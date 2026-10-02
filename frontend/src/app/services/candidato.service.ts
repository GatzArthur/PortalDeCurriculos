import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Candidato, CandidatoResumo, DadosExtraidos, NovoCandidato } from '../models/candidato';

@Injectable({ providedIn: 'root' })
export class CandidatoService {
  private http = inject(HttpClient);
  private base = '/api';   // o proxy do ng serve encaminha para o backend (ver proxy.conf.json)

  listar(busca?: string): Observable<CandidatoResumo[]> {
    let params = new HttpParams();
    if (busca?.trim()) params = params.set('busca', busca.trim());
    return this.http.get<CandidatoResumo[]>(`${this.base}/candidatos`, { params });
  }

  obter(id: number): Observable<Candidato> {
    return this.http.get<Candidato>(`${this.base}/candidatos/${id}`);
  }

  criar(dados: NovoCandidato): Observable<Candidato> {
    return this.http.post<Candidato>(`${this.base}/candidatos`, dados);
  }

  extrairDoPdf(arquivo: File): Observable<DadosExtraidos> {
    const form = new FormData();
    form.append('arquivo', arquivo);
    return this.http.post<DadosExtraidos>(`${this.base}/curriculos/extrair`, form);
  }
}
