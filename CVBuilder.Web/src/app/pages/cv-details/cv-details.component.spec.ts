import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CVDetailsComponent } from './cv-details.component';

describe('CvDetailsComponent', () => {
  let component: CVDetailsComponent;
  let fixture: ComponentFixture<CVDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CVDetailsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CVDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
