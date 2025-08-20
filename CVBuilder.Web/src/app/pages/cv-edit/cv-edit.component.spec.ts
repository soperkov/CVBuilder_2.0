import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CVEditComponent } from './cv-edit.component';

describe('CvEditComponent', () => {
  let component: CVEditComponent;
  let fixture: ComponentFixture<CVEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CVEditComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CVEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
