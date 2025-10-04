import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-create-version-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Создать новую версию</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
        <mat-label>Версия (например, 2.0.0)</mat-label>
        <input matInput [(ngModel)]="version" placeholder="2.0.0" required>
      </mat-form-field>

      <mat-form-field appearance="outline" style="width: 100%;">
        <mat-label>Описание</mat-label>
        <textarea 
          matInput 
          [(ngModel)]="comment" 
          placeholder="Что нового в этой версии?" 
          rows="3">
        </textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Отмена</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onCreate()"
        [disabled]="!version">
        Создать
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content {
      padding-top: 20px;
    }
  `]
})
export class CreateVersionDialogComponent {
  version = '';
  comment = '';

  constructor(private dialogRef: MatDialogRef<CreateVersionDialogComponent>) {}

  onCancel() {
    this.dialogRef.close();
  }

  onCreate() {
    if (this.version) {
      this.dialogRef.close({
        version: this.version,
        comment: this.comment
      });
    }
  }
}

