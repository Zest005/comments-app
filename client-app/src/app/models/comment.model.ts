export interface Comment {
  id: number;
  userName: string;
  email: string;
  homePage: string | null;
  text: string;
  createdAt: string;
  parentCommentId: number | null;
  attachment: Attachment | null;
  replies: Comment[];
}

export interface Attachment {
  id: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  url: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CaptchaResponse {
  captchaId: string;
  imageBase64: string;
}

export interface CreateCommentRequest {
  userName: string;
  email: string;
  homePage: string;
  text: string;
  parentCommentId: number | null;
  captchaText: string;
  captchaId: string;
}