import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-branch-dialog',
    standalone: true,
    imports: [CommonModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, FormsModule],
    templateUrl: './branch-dialog.component.html',
    styleUrl: './branch-dialog.component.scss'
})
export class BranchDialogComponent {
    constructor(@Inject(MAT_DIALOG_DATA) public data: any) { }
}
