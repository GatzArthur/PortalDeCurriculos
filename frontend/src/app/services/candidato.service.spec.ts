import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CandidatoService } from './candidato.service';

describe('CandidatoService', () => {
  let servico: CandidatoService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    servico = TestBed.inject(CandidatoService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('envia o PDF como multipart no campo "arquivo"', () => {
    servico.extrairDoPdf(new File(['%PDF-'], 'cv.pdf')).subscribe();
    const req = http.expectOne('/api/curriculos/extrair');
    expect(req.request.method).toBe('POST');
    expect((req.request.body as FormData).get('arquivo')).toBeTruthy();
    req.flush({});
  });

  it('inclui o termo de busca na listagem', () => {
    servico.listar(' maria ').subscribe();
    const req = http.expectOne(r => r.url === '/api/candidatos');
    expect(req.request.params.get('busca')).toBe('maria');
    req.flush([]);
  });
});
