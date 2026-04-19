import { Injectable, NgZone } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { Comment } from '../models/comment.model';

export interface NewReplyNotification {
  parentCommentId: number;
  replyId: number;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection!: signalR.HubConnection;
  public newComment$ = new Subject<Comment>();
  public newReply$ = new Subject<NewReplyNotification>();

  constructor(private ngZone: NgZone) {}

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/comments')
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR connection error:', err));

    this.hubConnection.on('NewComment', (comment: Comment) => {
      this.ngZone.run(() => {
        this.newComment$.next(comment);
      });
    });

    this.hubConnection.on('NewReply', (notification: NewReplyNotification) => {
      this.ngZone.run(() => {
        this.newReply$.next(notification);
      });
    });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}