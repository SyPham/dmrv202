import { Component, OnInit, ViewChild } from '@angular/core';
import { MailingService } from 'src/app/_core/_service/mailing.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { IMailing } from 'src/app/_core/_model/mailing';
import { BuildingUserService } from 'src/app/_core/_service/building.user.service';
import { DatePipe } from '@angular/common';
@Component({
  selector: 'app-mailing',
  templateUrl: './mailing.component.html',
  styleUrls: ['./mailing.component.scss'],
  providers: [DatePipe]
})
export class MailingComponent implements OnInit {

  mailing: any;
  data: IMailing[];
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  toolbarOptions = ['ExcelExport', 'Add', 'Edit', 'Delete', 'Cancel', 'Search'];
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  filterSettings = { type: 'Excel' };
  frequencyOption = ['Daily', 'Weekly', 'Monthly'];
  frequencyItem = '';
  reportOption = ['Done List', 'Cost'];
  reportItem = '';
  fields: object = { text: 'Username', value: 'ID' };
  userData: { ID: any; Username: any; Email: any; }[];
  box = 'Box';
  timeSend = new Date();
  datetimeSend = new Date();
  userIDList = [];
  userList = [];
  constructor(
    private mailingService: MailingService,
    private alertify: AlertifyService,
    private datePipe: DatePipe,
    private buildingUserService: BuildingUserService
  ) { }

  ngOnInit() {
    this.mailing = {
      id: 0,
      name: ''
    };
    this.getAllMailing();
  }
  // api
  getAllUsers() {
    this.buildingUserService.getAllUsers(1, 1000).subscribe(res => {
      const data = res.result.map((i: any) => {
        return {
          ID: i.ID,
          Username: i.Username,
          Email: i.Email,
        };
      });
      this.userData = data;
    });
  }
  getAllMailing() {
    this.mailingService.getAllMailing().subscribe(res => {
      this.data = res.map(item => {
        return {
          id: item.id,
          email: item.email,
          userID: item.userID,
          userName: item.userName,
          frequency: item.frequency,
          userNames: item.userNames,
          report: item.report,
          userIDList: item.userIDList,
          userList: item.userList,
          timeSend: new Date(item.timeSend),
        };
      });
      this.getAllUsers();
    });
  }
  create() {
    this.mailingService.create(this.mailing).subscribe(() => {
      this.alertify.success('Add Mailing Successfully');
      this.getAllMailing();
      this.mailing.name = '';
    });
  }
  onReport(args) {
    this.reportItem = args.value;
  }
  onFrequency(args) {
    this.frequencyItem = args.value;
  }
  onChangeTimeSend(time: Date) {
    this.timeSend = time;
  }
  onChangeDatetimeSend(args: any) {
    this.datetimeSend = args.value;
  }
  removing(args) {
    const filteredItems = this.userIDList.filter(item => item !== args.itemData.ID);
    this.userIDList = filteredItems;
    this.userList = this.userList.filter(item => item.id !== args.itemData.ID);
  }
  onSelectUsername(args) {
    const data = args.itemData;
    this.userIDList.push(data.ID);
    this.userList.push({ mailingID: 0 , id: data.ID, email: data.Email});
  }
  update() {
    this.mailingService.update(this.mailing).subscribe(() => {
      this.alertify.success('Add Mailing Successfully');
      this.getAllMailing();
      this.mailing.name = '';
    });
  }
  delete(id) {
    this.alertify.confirm('Delete Mailing', 'Are you sure you want to delete this Mailing "' + id + '" ?', () => {
      this.mailingService.delete(id).subscribe(() => {
        this.getAllMailing();
        this.alertify.success('The Mailing has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the Mailing');
      });
    });
  }
  createRange(obj) {
    this.mailingService.createRange(obj).subscribe(() => {
      this.alertify.success('Add Mailing Successfully');
      this.getAllMailing();
      this.mailing.name = '';
    }, err => {
        this.alertify.error(err);
        this.getAllMailing();
    });
  }
  updateRange(obj) {
    this.mailingService.updateRange(obj).subscribe(() => {
      this.alertify.success('Add Mailing Successfully');
      this.getAllMailing();
      this.mailing.name = '';
    }, err => {
      this.alertify.error(err);
      this.getAllMailing();
    });
  }
  // end api

  // grid event
  toolbarClick(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case 'Excel Export':
        this.grid.excelExport();
        break;
      /* tslint:enable */
      default:
        break;
    }
  }
  actionBegin(args) {
    if (args.requestType === 'beginEdit') {
      if (args.requestType === 'add') {
        this.reportItem = '';
        this.frequencyItem = '';
        this.timeSend = new Date();
        this.datetimeSend = new Date();
      }
      const data = args.rowData;
      this.userIDList = data.userIDList;
      this.reportItem = data.report;
      this.frequencyItem = data.frequency;
      this.timeSend = data.timeSend;
      this.datetimeSend = data.timeSend;
      this.userIDList = data.userIDList;
      this.userList = data.userList;
      if (data.frequency === 'Daily') {
        this.timeSend = data.timeSend;
      } else {
        this.datetimeSend = data.timeSend;
      }
    }
    if (args.requestType === 'save') {
      if (args.action === 'add') {
        const obj = [];
        for (const item of this.userList) {
          const data = {
            report: this.reportItem,
            frequency: this.frequencyItem,
            timeSend: this.frequencyItem === 'Daily' ?
            this.datePipe.transform(this.timeSend, 'MM-dd-yyyy HH:mm') : this.datePipe.transform(this.datetimeSend, 'MM-dd-yyyy HH:mm'),
            userID: item.id,
            id: item.mailingID,
            email: item.email
          };
          obj.push(data);
        }
        this.createRange(obj);
      }
      if (args.action === 'edit') {
        const obj = [];
        for (const item of this.userList) {
          const data = {
            report: this.reportItem,
            frequency: this.frequencyItem,
            timeSend: this.frequencyItem === 'Daily' ?
              this.datePipe.transform(this.timeSend, 'MM-dd-yyyy HH:mm') : this.datePipe.transform(this.datetimeSend, 'MM-dd-yyyy HH:mm'),
            userID: item.id,
            id: item.mailingID,
            email: item.email
          };
          obj.push(data);
        }
        this.updateRange(obj);
      }
    }
    // if (args.requestType === 'delete') {
    //   this.delete(args.data[0].id);
    // }
  }
  actionComplete(e: any): void {
    if (e.requestType === 'add') {
      // (e.form.elements.namedItem('name') as HTMLInputElement).focus();
      (e.form.elements.namedItem('id') as HTMLInputElement).disabled = true;
    }
  }
  // end event
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.grid.pageSettings.pageSize + Number(index) + 1;
  }
}
