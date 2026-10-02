export interface CandidatoResumo {
  id: number;
  nomeCompleto: string;
  email: string;
  telefone?: string | null;
  areaInteresse?: string | null;
  criadoEm: string;
}

export interface Candidato extends CandidatoResumo {
  resumoProfissional?: string | null;
}

export interface NovoCandidato {
  nomeCompleto: string;
  email: string;
  telefone: string;
  areaInteresse: string;
  resumoProfissional: string;
}

export interface DadosExtraidos {
  nomeCompleto?: string | null;
  email?: string | null;
  telefone?: string | null;
}
