import { TestBed } from '@angular/core/testing';

import { RateService } from './_services/rate.service';

describe('RateService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: RateService = TestBed.get(RateService);
    expect(service).toBeTruthy();
  });
});
