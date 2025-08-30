import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CVService } from '../../services/cv.service';
import { Cv } from '../../models';
import { toLocalDate } from '../../utils/date.utils';

type CvDisplay = Cv & {
  localTime: Date | null;
  templateName?: string;
};

@Component({
  selector: 'app-my-cvs',
  standalone: false,
  templateUrl: './my-cvs.component.html',
  styleUrls: ['./my-cvs.component.css'],
})
export class MyCVsComponent implements OnInit {
  cvs: CvDisplay[] = [];
  loading = true;
  error = '';

  // selection state
  selectionMode = false;
  selectedIds = new Set<number>();
  showDeleteModal = false;

  constructor(private cvService: CVService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.cvService.getMyCVs().subscribe({
      next: (res: Cv[]) => {
        this.cvs = (res || [])
          .map((cv: any) => {
            const when = cv.updatedAt ?? cv.createdAt;
            return {
              ...cv,
              localTime: toLocalDate(when),
              templateName: cv.templateName ?? cv.template?.name,
            };
          })
          .sort(
            (a, b) =>
              (b.localTime?.getTime() ?? 0) - (a.localTime?.getTime() ?? 0)
          );
        this.loading = false;
        this.selectionMode = false;
        this.selectedIds.clear();
      },
      error: () => {
        this.error = 'Failed to load CVs';
        this.loading = false;
      },
    });
  }

  // navigation
  viewDetails(id: number) {
    if (!this.selectionMode) this.router.navigate(['/cv', id]);
  }
  editCV(id: number) {
    if (!this.selectionMode) this.router.navigate(['/cv/edit', id]);
  }

  // selection
  toggleSelectionMode(): void {
    this.selectionMode = !this.selectionMode;
    if (!this.selectionMode) this.selectedIds.clear();
  }
  isSelected(id: number): boolean {
    return this.selectedIds.has(id);
  }
  toggleItem(id: number, ev?: MouseEvent): void {
    ev?.stopPropagation();
    if (!this.selectionMode) return;
    this.selectedIds.has(id)
      ? this.selectedIds.delete(id)
      : this.selectedIds.add(id);
  }

  // delete
  openDeleteModal(): void {
    if (this.selectedIds.size > 0) this.showDeleteModal = true;
  }
  closeDeleteModal(): void {
    this.showDeleteModal = false;
  }
  confirmDelete(): void {
    const ids = Array.from(this.selectedIds);
    this.cvService.deleteMany(ids).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.selectedIds.clear();
        this.selectionMode = false;
        this.load();
      },
      error: () => {
        this.error = 'Failed to delete selected CV(s).';
        this.closeDeleteModal();
      },
    });
  }

  // at top-level of the component class
  cardVars: Array<{
    mx: number;
    my: number;
    rX: number;
    rY: number;
    elev: number;
  }> = [];

  onCardMove(e: MouseEvent, i: number) {
    const el = e.currentTarget as HTMLElement;
    const r = el.getBoundingClientRect();
    const x = e.clientX - r.left;
    const y = e.clientY - r.top;
    const px = x / r.width - 0.5; // -0.5 .. 0.5
    const py = y / r.height - 0.5;

    const rY = -px * 10; // rotateY
    const rX = py * 8; // rotateX
    const elev = 6 + Math.hypot(px, py) * 10; // box-shadow lift

    this.cardVars[i] = { mx: x, my: y, rX, rY, elev };
  }

  resetCard(i: number) {
    this.cardVars[i] = { mx: 0, my: 0, rX: 0, rY: 0, elev: 0 };
  }

  trackById = (_: number, item: CvDisplay) => item.id;
}
