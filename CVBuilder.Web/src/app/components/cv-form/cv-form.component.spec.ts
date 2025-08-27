import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CVFormComponent } from './cv-form.component';

describe('CvFormComponent', () => {
  let component: CVFormComponent;
  let fixture: ComponentFixture<CVFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CVFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CVFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
