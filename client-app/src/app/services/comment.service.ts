import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Comment, PagedResult, CaptchaResponse } from '../models/comment.model';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = '/api/comments';
  private captchaUrl = '/api/captcha';

  constructor(private http: HttpClient) {}

  getComments(
    page: number = 1,
    pageSize: number = 25,
    sortBy: string = 'createdAt',
    sortDescending: boolean = true
  ): Observable<PagedResult<Comment>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortBy', sortBy)
      .set('sortDescending', sortDescending.toString());

    return this.http.get<PagedResult<Comment>>(this.apiUrl, { params });
  }

  getReplies(parentId: number, skip: number = 0, take: number = 3): Observable<PagedResult<Comment>> {
    const params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());
    return this.http.get<PagedResult<Comment>>(`${this.apiUrl}/${parentId}/replies`, { params });
  }

  createComment(
    data: {
      userName: string;
      email: string;
      homePage: string;
      text: string;
      parentCommentId: number | null;
      captchaText: string;
      captchaId: string;
    },
    file: File | null
  ): Observable<Comment> {
    const formData = new FormData();
    formData.append('userName', data.userName);
    formData.append('email', data.email);
    formData.append('homePage', data.homePage || '');
    formData.append('text', data.text);
    formData.append('captchaText', data.captchaText);
    formData.append('captchaId', data.captchaId);

    if (data.parentCommentId !== null) {
      formData.append('parentCommentId', data.parentCommentId.toString());
    }

    if (file) {
      formData.append('file', file);
    }

    return this.http.post<Comment>(this.apiUrl, formData);
  }

  getCaptcha(): Observable<CaptchaResponse> {
    return this.http.get<CaptchaResponse>(this.captchaUrl);
  }
}