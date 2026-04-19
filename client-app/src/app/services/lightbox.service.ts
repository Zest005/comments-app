import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LightboxService {
  private openSubject = new Subject<string>();
  public open$ = this.openSubject.asObservable();

  open(imageUrl: string): void {
    this.openSubject.next(imageUrl);
  }
}