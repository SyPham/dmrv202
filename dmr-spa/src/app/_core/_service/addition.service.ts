import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class AdditionService {
  baseUrl = environment.apiUrlEC;
  ModalNameSource = new BehaviorSubject<number>(0);
  currentModalName = this.ModalNameSource.asObservable();
  constructor(
    private http: HttpClient
  ) { }

  getAllAddition() {
    return this.http.get<any>(this.baseUrl + 'Addition/GetAll', {});
  }
  getAllChemical() {
    return this.http.get<any>(this.baseUrl + 'Addition/getAllChemical', {});
  }
  getAllByBuildingID(id) {
    return this.http.get<any>(this.baseUrl + 'Addition/GetAllByBuildingID/' + id, {});
  }
  getLinesByBuildingID(id) {
    return this.http.get<any>(this.baseUrl + 'Addition/GetLinesByBuildingID/' + id, {});
  }
  getBPFCSchedulesByApprovalStatus() {
    return this.http.get<any>(this.baseUrl + 'Addition/GetBPFCSchedulesByApprovalStatus', {});
  }
  create(model) {
    return this.http.post(this.baseUrl + 'Addition/CreateRange', model);
  }
  update(model) {
    return this.http.put(this.baseUrl + 'Addition/UpdateRange', model);
  }
  delete(idList: [], deleteBy) {
     const list = idList || [];
     let query = '';
     for (const id of list) {
      query += `idList=${id}&`
     }
     const deleteByQuery = `deleteBy=${deleteBy}`;
    return this.http.delete(this.baseUrl + `Addition/DeleteRange?${query}${deleteByQuery}`);
  }
 
}
