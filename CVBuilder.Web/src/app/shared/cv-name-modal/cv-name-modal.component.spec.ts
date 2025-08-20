import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CvNameModalComponent } from './cv-name-modal.component';

describe('CvNameModalComponent', () => {
  let component: CvNameModalComponent;
  let fixture: ComponentFixture<CvNameModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CvNameModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CvNameModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
