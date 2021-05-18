import { Injectable } from '@angular/core';
import { HttpRequest, HttpErrorResponse, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { AlertifyService } from '../_service/alertify.service';
import { AuthService } from '../_service/auth.service';

@Injectable()
export class BasicAuthInterceptor implements HttpInterceptor {
    constructor(private alertify: AlertifyService, private authSerive: AuthService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // add authorization header with basic auth credentials if available
        if (localStorage.getItem('token')) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });
        }
        // return next.handle(request);
        return next.handle(request).pipe(
            retry(1),
            catchError((error: HttpErrorResponse) => {
                let errorMessage = '';
                if (error.error instanceof ErrorEvent) {
                    // client-side error
                    errorMessage = `Error: ${error.error.message}`;
                } else {
                    // server-side error
                    errorMessage = `Error Status: ${error.status}\nMessage: ${error.message}`;
                    switch (error.status) {
                        case 0:
                            alert(`
                            Hệ thống không hoạt động vì lỗi mạng. Vui lòng nhấn F5 để thử lại hoặc liên hệ phòng IT.
                            The system does not work due to network error. Please press F5 to try again or contact IT department!
                            `);
                            return throwError(errorMessage);
                          case 401:
                            alert(`
                            Phiên làm việc của bạn đã hết hạn! Vui lòng đăng nhập lại để tiếp tục làm việc.
                            The token invalid! Please login again!
                            `);
                            this.authSerive.logOutAuth();
                            return throwError(error.error || errorMessage);
                          case 400:
                            this.alertify.error(error.error || errorMessage);
                            return throwError(error.error);
                          case 500:
                            alert("Máy chủ đang gặp vấn đề. Vui lòng thử lại sau! The server error. Please try again after sometime!");
                            return throwError(error);
                    }
                }
                return throwError(errorMessage);
            })
        );
    }
}
