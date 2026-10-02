# Registro de desenvolvimento

## Como organizei o trabalho

Ordem seguida: 
    (1) leitura do enunciado e lista de requisitos; 
    (2) modelagem do banco e script SQL; 
    (3) backend: modelo, validações, endpoints de candidatos; 
    (4) leitura do PDF e extração de dados; 
    (5) frontend: formulário único, listagem, detalhes; 
    (6) testes; 
    (7) documentação.


## Principais decisões técnicas

- **Leitura do PDF no backend com PdfPig:** biblioteca .NET pura, sem dependência nativa nem serviço externo, o que facilita rodar em qualquer máquina.
- **Endpoint de extração separado (`/api/curriculos/extrair`) que não grava nada:** o PDF só sugere valores; quem salva é sempre o `POST /api/candidatos`. Assim os dois caminhos passam pela **mesma validação**, e uma falha no PDF não afeta o cadastro.
- **Extração por regex/heurística:** regex resolve com simplicidade e é fácil de testar e evoluir. Nome = primeira linha plausível entre as 8 primeiras.
- **Validação em duas camadas com as mesmas regras:** DataAnnotations no backend (fonte da verdade) e Reactive Forms no frontend (feedback imediato). O frontend também exibe erros de campo devolvidos pelo servidor.
- **Validação do PDF pelo conteúdo:** além da extensão e do limite de 5 MB, confere o cabeçalho `%PDF-`.
- **SQL Server com script SQL idempotente** em vez de migrations do EF: o modelo é uma única tabela e o script é fácil de executar em qualquer ambiente. (Alternativa futura: migrations do EF Core.)
- **Proxy do `ng serve`** para `/api`: evita configurar CORS no desenvolvimento (o CORS também está liberado para `localhost:4200`).


## Uso de IA

- **Ferramenta/modelo:** Claude (Anthropic), via chat.
- **Onde ajudou:** comparação Angular x React, esqueleto do backend e do frontend, regex de extração, testes e rascunho da documentação.
- **Exemplos de pedidos:** Solicitei auxilio pois as datas de criações dos candidatos estavam sendo salvas no modelo UTC, o que adianta o fusohorário em que estamos; Também solicitei auxilio nos testes da aplicação, na geração de curriculos ficticios e possibilidades de erros.


## Como verifiquei a solução

- Testes automatizados: backend (xUnit: validação do modelo, validação de arquivo, parser, leitura do PDF de exemplo, controllers) e frontend (Jasmine: validações do formulário, fluxo de PDF, falha de leitura não bloqueando o cadastro manual, serviço HTTP).
- Teste manual ponta a ponta: cadastro manual, cadastro com `samples/curriculo-ficticio.pdf`, arquivo inválido, arquivo > 5 MB, PDF sem texto, backend fora do ar.


## Tempo dedicado

- Entre 14 e 16 horas


## Dificuldades, limitações e melhorias futuras

- **Ambiente:** Percebi que o `<` (redirecionamento) do bash não funciona no PowerShell, então o README usa `docker cp` + `docker exec` para o Windowa.
- Guardar o PDF original e o texto extraído junto ao candidato.
- Paginação na listagem, edição e exclusão de candidatos, verificação de e-mail duplicado.
- Autenticação para a equipe de recrutamento e atenção à LGPD (consentimento e exclusão de dados).
