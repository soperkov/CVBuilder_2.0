import { Component, OnInit } from '@angular/core';
import { CVService } from '../../services/cv.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-my-cvs',
  standalone: false,
  templateUrl: './my-cvs.component.html',
  styleUrls: ['./my-cvs.component.css'],
})
export class MyCVsComponent implements OnInit {
  cvs: any[] = [];
  loading = true;
  error = '';

  constructor(private cvService: CVService, private router: Router) {}

  ngOnInit(): void {
    this.cvService.getMyCVs().subscribe({
      next: (res) => {
        this.cvs = res;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load CVs';
        this.loading = false;
      },
    });
  }

  viewDetails(id: number) {
    this.router.navigate(['/cv', id]);
  }

  editCV(id: number) {
    this.router.navigate(['/cv/edit', id]);
  }
}
