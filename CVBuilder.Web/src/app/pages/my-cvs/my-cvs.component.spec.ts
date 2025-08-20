import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyCVsComponent } from './my-cvs.component';

describe('MyCvsComponent', () => {
  let component: MyCVsComponent;
  let fixture: ComponentFixture<MyCVsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MyCVsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MyCVsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
