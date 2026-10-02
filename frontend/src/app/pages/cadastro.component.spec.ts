import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CandidatoService } from '../services/candidato.service';
import { CadastroComponent } from './cadastro.component';
import { Candidato } from '../models/candidato';

function evento(arquivo: File): Event {
  return { target: { files: [arquivo], value: 'x' } } as unknown as Event;
}

describe('CadastroComponent', () => {
  let componente: CadastroComponent;
  let servico: CandidatoService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CadastroComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    });
    componente = TestBed.createComponent(CadastroComponent).componentInstance;
    servico = TestBed.inject(CandidatoService);
  });

  it('exige nome e e-mail', () => {
    expect(componente.form.valid).toBeFalse();
    componente.form.patchValue({ nomeCompleto: 'Maria Silva', email: 'maria@exemplo.com' });
    expect(componente.form.valid).toBeTrue();
  });

  it('rejeita nome só com espaços e e-mail em formato inválido', () => {
    componente.form.patchValue({ nomeCompleto: '   ', email: 'sem-arroba' });
    expect(componente.form.controls.nomeCompleto.errors?.['obrigatorio']).toBeTrue();
    expect(componente.form.controls.email.errors?.['pattern']).toBeTruthy();
  });

  it('rejeita arquivo que não é PDF sem chamar o backend', () => {
    const espiao = spyOn(servico, 'extrairDoPdf');
    componente.aoEscolherArquivo(evento(new File(['x'], 'cv.docx')));
    expect(espiao).not.toHaveBeenCalled();
    expect(componente.aviso()?.tipo).toBe('erro');
  });

  it('rejeita PDF maior que 5 MB', () => {
    const espiao = spyOn(servico, 'extrairDoPdf');
    const grande = new File([new Uint8Array(5 * 1024 * 1024 + 1)], 'cv.pdf');
    componente.aoEscolherArquivo(evento(grande));
    expect(espiao).not.toHaveBeenCalled();
    expect(componente.aviso()?.texto).toContain('5 MB');
  });

  it('preenche o formulário com os dados extraídos do PDF', () => {
    spyOn(servico, 'extrairDoPdf').and.returnValue(of({ nomeCompleto: 'Ana Souza', email: 'ana@exemplo.com', telefone: null }));
    componente.aoEscolherArquivo(evento(new File(['%PDF-'], 'cv.pdf')));
    expect(componente.form.value.nomeCompleto).toBe('Ana Souza');
    expect(componente.form.value.email).toBe('ana@exemplo.com');
    expect(componente.form.value.telefone).toBe('');
  });

  it('falha na leitura do PDF mostra erro mas não impede o cadastro manual', () => {
    const erro = new HttpErrorResponse({ status: 422, error: { detail: 'Não foi possível ler o PDF.' } });
    spyOn(servico, 'extrairDoPdf').and.returnValue(throwError(() => erro));
    const criar = spyOn(servico, 'criar').and.returnValue(of({ id: 1 } as Candidato));

    componente.aoEscolherArquivo(evento(new File(['%PDF-'], 'cv.pdf')));
    expect(componente.aviso()?.tipo).toBe('erro');

    componente.form.patchValue({ nomeCompleto: 'Maria Silva', email: 'maria@exemplo.com' });
    componente.salvar();
    expect(criar).toHaveBeenCalled();
  });
});
