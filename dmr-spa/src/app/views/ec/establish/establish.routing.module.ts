import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AuthGuard } from "src/app/_core/_guards/auth.guard";
import { BPFCScheduleComponent, BpfcDetailComponent,
  BpfcStatusComponent, Bpfc1Component } from ".";
const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'bpfc-schedule',
        data: {
          title: 'bpfc-schedule',
          breadcrumb: 'BPFC Schedule',
          functionCode: 'BPFC Schedule'
        },
        canActivate: [AuthGuard],
        children: [
          {
            path: '',
            component: BPFCScheduleComponent
          },
          {
            path: 'detail',
            data: {
              title: 'Detail',
              breadcrumb: 'Detail'
            },
            children: [
              {
                path: '',
                component: BpfcDetailComponent,
              },
              {
                path: ':id',
                component: BpfcDetailComponent,
              }
            ]
          },

        ]
      },
      {
        path: 'bpfc-status',
        component: BpfcStatusComponent,
        data: {
          title: 'BPFC Status',
          breadcrumb: 'BPFC Status',
          functionCode: 'BPFC Status'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'bpfc-1',
        component: Bpfc1Component,
        data: {
          title: 'BPFC ',
          breadcrumb: 'BPFC',
          functionCode: 'BPFC'
        },
        canActivate: [AuthGuard]
      }
    ]
  }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EstablishRoutingModule { }
