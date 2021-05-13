import { PermissionService } from 'src/app/_core/_service/permission.service';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { AuthService } from '../../_core/_service/auth.service';
import { AlertifyService } from '../../_core/_service/alertify.service';
import { Router, ActivatedRoute, RouterStateSnapshot } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { UserForLogin } from 'src/app/_core/_model/user';
import { environment } from 'src/environments/environment';
import { RoleService } from 'src/app/_core/_service/role.service';
import { IRole, IUserRole } from 'src/app/_core/_model/role';
import { IBuilding } from 'src/app/_core/_model/building';
import { AuthenticationService } from 'src/app/_core/_service/authentication.service';
import { Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { FunctionSystem } from 'src/app/_core/_model/application-user';
const ADMIN = 1;
const ADMIN_COSTING = 5;
const SUPERVISOR = 2;
const STAFF = 3;
const WORKER = 4;
const WORKER2 = 6;
const DISPATCHER = 6;
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy {
  busy = false;
  username = '';
  password = '';
  loginError = false;
  private subscription: Subscription;

  user: UserForLogin = {
    username: '',
    password: '',
    systemCode: environment.systemCode
  };
  uri: any;
  level: number;

  remember = false;
  functions: FunctionSystem[];
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthenticationService,
    private permisisonService: PermissionService,
    private roleService: RoleService,
    private cookieService: CookieService,
    private alertifyService: AlertifyService
  ) {
    if (this.cookieService.get('remember') !== undefined) {
      if (this.cookieService.get('remember') === 'Yes') {
        this.user = {
          username: this.cookieService.get('username'),
          password: this.cookieService.get('password'),
          systemCode: environment.systemCode
        };
        this.username = this.cookieService.get('username');
        this.password = this.cookieService.get('password');
        this.remember = true;
        this.login();
      }
    }
    this.uri = this.route.snapshot.queryParams.uri || '/ec/execution/todolist-2';
  }
  role: number;
  ngOnInit(): void {
    const accessToken = localStorage.getItem('token');
    const refreshToken = localStorage.getItem('refresh_token');
    if (accessToken && refreshToken && this.route.routeConfig.path === 'login') {
      const uri = decodeURI(this.uri) || '/ec/execution/todolist-2';
      this.router.navigate([uri]);
    }
    // this.subscription = this.authService.user$.subscribe((x) => {
    //   if (this.route.snapshot.url[0].path === 'login') {
    //     const accessToken = localStorage.getItem('access_token');
    //     const refreshToken = localStorage.getItem('refresh_token');
    //     if (x && accessToken && refreshToken) {
    //       const returnUrl = this.route.snapshot.queryParams.returnUrl || '/ec/execution/todolist-2';
    //       this.router.navigate([returnUrl]);
    //     }
    //   } // optional touch-up: if a tab shows login page, then refresh the page to reduce duplicate login
    // });
  }
  onChangeRemember(args) {
    this.remember = args.target.checked;
  }
  login() {
    if (!this.username || !this.password) {
      return;
    }
    this.busy = true;
    const returnUrl = this.route.snapshot.queryParams.returnUrl || '/ec/execution/todolist-2';
    this.authService
      .login(this.username, this.password)
      .pipe(
        finalize(() => (this.busy = false))
        )
      .subscribe(
        async () => {
          this.role = JSON.parse(localStorage.getItem('user')).user.role;
          const userId = JSON.parse(localStorage.getItem('user')).user.id;

          const res = await this.authService.getBuildingUserByUserID(userId).toPromise();
          localStorage.setItem('building', JSON.stringify(res.data));
          this.authService.setBuildingValue(res.data as IBuilding[]);

          const roleUser = await this.roleService.getRoleByUserID(userId).toPromise();
          localStorage.setItem('level', JSON.stringify(roleUser));
          this.authService.setRoleValue(roleUser as IRole);

          const userRole: IUserRole = {
            isLock: true,
            userID: userId,
            roleID: roleUser.id
          };
          this.roleService.isLock(userRole).subscribe((isLock: boolean) => {
            if (isLock) {
              this.alertifyService.error('Your account has been locked!!!');
              return;
            }
          });
          const functions = await this.permisisonService.getActionInFunctionByRoleID(roleUser.id).toPromise();
          this.functions = functions as FunctionSystem[];
          localStorage.setItem("functions", JSON.stringify(functions));
          this.authService.setFunctions(functions as any);
          this.alertifyService.success('Login Success!!');

          const currentLang = localStorage.getItem('lang');
          if (currentLang) {
            localStorage.setItem('lang', currentLang);
          } else {
            localStorage.setItem('lang', 'vi');
          }

          if (this.remember) {
            this.cookieService.set('remember', 'Yes');
            this.cookieService.set('username', this.user.username);
            this.cookieService.set('password', this.user.password);
            this.cookieService.set('systemCode', this.user.systemCode.toString());
          } else {
            this.cookieService.set('remember', 'No');
            this.cookieService.set('username', '');
            this.cookieService.set('password', '');
            this.cookieService.set('systemCode', '');
          }
          setTimeout(() => {
            const check = this.checkRole();
            if (check) {
              const uri = decodeURI(this.uri);
              this.router.navigate([uri]);
            } else {
              this.router.navigate(['/ec/execution/todolist-2']);
            }

          });
        },
        () => {
          this.loginError = true;
        }
      );
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  checkRole(): boolean {
    const uri = decodeURI(this.uri);
    const permissions = this.functions.map(x => x.url);
    for (const url of permissions) {
      if (uri.includes(url)) {
        return true;
      }
    }
    return false;

  }
}
