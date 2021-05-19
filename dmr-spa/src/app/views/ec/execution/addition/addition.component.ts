import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { AdditionService } from 'src/app/_core/_service/addition.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { BaseComponent } from 'src/app/_core/_component/base.component';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { NgForOf } from '@angular/common';

@Component({
  selector: 'app-addition',
  templateUrl: './addition.component.html',
  styleUrls: ['./addition.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class AdditionComponent extends BaseComponent implements OnInit {
  addition = {
    id: 0,
    bpfcEstablishID: 0,
    lineIDList: [],
    idList: [],
    lineID: 0,
    chemicalID: 0,
    remark: '',
    workerID: '',
    amount: 0,
    createdBy: 0,
    isDelete: false,
    modifiedBy: 0
  };
  data: any = [];
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  fieldsChemical: object = { text: 'name', value: 'name' };
  fieldsLine: object = { text: 'name', value: 'id' };
  fieldsBPFC: object = { text: 'name', value: 'name' };
  filterSettings = { type: 'Excel' };
  bpfcData: any = [];
  bpfcEstablishID = 0;
  lineID = 0;
  chemicalID = 0;
  chemicalData: any = [];
  lineData: any = [];
  lines: any[] = [];
  addData: any[];
  editData: any[];
  constructor(
    public activeModal: NgbActiveModal,
    private additionService: AdditionService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) {
    super();
  }

  ngOnInit() {
    this.Permission(this.route);
    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
    this.toolbarOptions = ['Add', 'Edit', 'Delete', 'Cancel', 'Search'];

    this.getAllByBuildingID();
    this.getAllChemical();
    this.getLinesByBuildingID();
    this.getBPFCSchedulesByApprovalStatus();
  }
  // api
  getAllByBuildingID() {
    const buildingID = +localStorage.getItem('buildingID') || 0;
    this.additionService.getAllByBuildingID(buildingID).subscribe(res => {
      this.data = res;
    });
  }
  getAllChemical() {
    this.additionService.getAllChemical().subscribe(res => {
      this.chemicalData = res.map(x => {
        return {
          id: x.id,
          name: x.name
        }
      });
    });
  }
  getBPFCSchedulesByApprovalStatus() {
    this.additionService.getBPFCSchedulesByApprovalStatus().subscribe(res => {
      this.bpfcData = res.map(x => {
        return {
          id: x.id,
          name: `${x.modelName} - ${x.modelNo} - ${x.articleNo} - ${x.artProcess}`
        };
      });
    });
  }
  getLinesByBuildingID() {
    const buildingID = +localStorage.getItem('buildingID') || 0;
    this.lineData = this.additionService.getLinesByBuildingID(buildingID).subscribe(res => {
      this.lineData = res.map(x => {
        return {
          id: x.id,
          name: x.name
        }
      });
    });
  }
  initModel() {
    this.addition = {
      id: 0,
      bpfcEstablishID: 0,
      lineIDList: [],
      lineID: 0,
      chemicalID: 0,
      remark: '',
      workerID: '',
      amount: 0,
      idList: [],
      isDelete: false,
      createdBy: +JSON.parse(localStorage.getItem('user')).user.id,
      modifiedBy: +JSON.parse(localStorage.getItem('user')).user.id
    };
    this.chemicalID = 0;
    this.lineID = 0;
    this.bpfcEstablishID = 0;
    this.lines = [];
  }
  create() {
    this.additionService.create(this.addData).subscribe(() => {
      this.alertify.success('Add Addition Successfully');
      this.getAllByBuildingID();
      this.initModel();
    });
  }

  update() {
    this.additionService.update(this.addition).subscribe(() => {
      this.alertify.success('Add Addition Successfully');
      // this.modalReference.close() ;
      this.getAllByBuildingID();
      this.initModel();
    });
  }
  delete(id) {
    const deleteBy = +JSON.parse(localStorage.getItem('user')).user.id;
    this.alertify.confirm('Delete Addition', 'Are you sure you want to delete this Addition "' + id + '" ?', () => {
      this.additionService.delete(id, deleteBy).subscribe(() => {
        this.getAllByBuildingID();
        this.alertify.success('The addition has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the addition');
      });
    });
  }
  deleteWhenUpdate(id) {
    const deleteBy = +JSON.parse(localStorage.getItem('user')).user.id;
    this.additionService.delete(id, deleteBy).subscribe(() => { });
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
    this.grid.toolbarModule.getToolbar();
    if (args.requestType === 'add') {
      this.initModel();
      // let item = args.rowData;
      // item = Object.assign({}, this.addition);
    }
    if (args.requestType === 'beginEdit') {
      const item = args.rowData;
      this.chemicalID = item.chemicalID;
      this.lineID = item.lineID;
      this.lines = item.lineIDList;
      this.bpfcEstablishID = item.bpfcEstablishID;
    }
    if (args.requestType === 'save') {
      if (args.action === 'add') {

        const data = args.data;
        if (this.lines.length > 0) {
          this.addData = [];
          this.addition.id = 0;
          this.addition.remark = data.remark;
          this.addition.amount = data.amount;
          this.addition.chemicalID = this.chemicalID;
          this.addition.bpfcEstablishID = this.bpfcEstablishID;
          this.addition.lineID = this.lineID;
          this.addition.workerID = data.workerID;
          for (const lineID of this.lines) {
            const itemAdd = Object.assign({}, this.addition);
            itemAdd.lineID = lineID;
            this.addData.push(itemAdd);
          }
          this.create();

        }
      }
      if (args.action === 'edit') {
        const data = args.data;
        this.addition.id = data.id;
        this.addition.remark = data.remark;
        this.addition.workerID = data.workerID;
        this.addition.amount = data.amount;
        this.addition.lineID = this.lineID;
        this.addition.chemicalID = this.chemicalID;
        this.addition.bpfcEstablishID = this.bpfcEstablishID;
        this.addition.idList = data.idList;
        this.addition.lineIDList = this.lines;
        this.update();
      }
    }
    if (args.requestType === 'delete') {
      this.delete(args.data[0].idList);
    }
  }
  actionComplete(e: any): void {
    // if (e.requestType === 'add') {
    //   (e.form.elements.namedItem('name') as HTMLInputElement).focus();
    //   (e.form.elements.namedItem('id') as HTMLInputElement).disabled = true;
    // }
  }
  // end event
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.pageSettings.pageSize + Number(index) + 1;
  }
  onChangeChemical(args) {
    this.chemicalID = args.itemData.id;
  }

  onChangeLine(args) {
    this.lineID = args.itemData.id;
  }
  onChangeBPFC(args) {
    this.bpfcEstablishID = args.itemData.id;
  }
  removing(args) {
    console.log(args);
    // const filteredItems = this.lineIDList.filter(item => item !== args.itemData.id);
    // this.lineIDList = filteredItems;
    // this.lineList = this.userList.filter(item => item.id !== args.itemData.id);
  }
  onSelectUsername(args) {
    console.log(args);
    const data = args.itemData;
    // this.userIDList.push(data.ID);
    // this.userList.push({ mailingID: 0 , id: data.id, email: data.Email});
  }
}
