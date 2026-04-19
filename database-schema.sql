CREATE DATABASE IF NOT EXISTS CommentsAppDb;
USE CommentsAppDb;

CREATE TABLE Comments (
    Id              INT             NOT NULL AUTO_INCREMENT,
    UserName        NVARCHAR(30)    NOT NULL,
    Email           NVARCHAR(255)   NOT NULL,
    HomePage        NVARCHAR(2048)  NULL,
    Text            NVARCHAR(MAX)   NOT NULL,
    CreatedAt       DATETIME2(7)    NOT NULL,
    ParentCommentId INT             NULL,

    CONSTRAINT PK_Comments PRIMARY KEY (Id),
    CONSTRAINT FK_Comments_ParentComment 
        FOREIGN KEY (ParentCommentId) 
        REFERENCES Comments(Id)
        ON DELETE NO ACTION
);

CREATE TABLE CommentAttachments (
    Id              INT             NOT NULL AUTO_INCREMENT,
    CommentId       INT             NOT NULL,
    FileName        NVARCHAR(255)   NOT NULL,
    StoredFilePath  NVARCHAR(2048)  NOT NULL,
    ContentType     NVARCHAR(255)   NOT NULL,
    FileSize        BIGINT          NOT NULL,

    CONSTRAINT PK_CommentAttachments PRIMARY KEY (Id),
    CONSTRAINT FK_CommentAttachments_Comment 
        FOREIGN KEY (CommentId) 
        REFERENCES Comments(Id)
        ON DELETE CASCADE
);

CREATE INDEX IX_Comments_CreatedAt ON Comments(CreatedAt);
CREATE INDEX IX_Comments_UserName ON Comments(UserName);
CREATE INDEX IX_Comments_Email ON Comments(Email);
CREATE INDEX IX_Comments_ParentCommentId ON Comments(ParentCommentId);