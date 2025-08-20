import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CVService } from '../../services/cv.service';

@Component({
  selector: 'app-cv-details',
  standalone: false,
  templateUrl: './cv-details.component.html',
  styleUrls: ['./cv-details.component.css'],
})
export class CVDetailsComponent implements OnInit {
  cv: any;
  loading = true;
  error = '';

  constructor(private route: ActivatedRoute, private cvService: CVService) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.cvService.getCVById(id).subscribe({
      next: (res) => {
        this.cv = res;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load CV details.';
        this.loading = false;
      },
    });
  }
}
