# Portal de Currículos

Aplicação para a equipe de recrutamento cadastrar e consultar candidatos, com dois caminhos de cadastro que usam **o mesmo formulário e as mesmas validações**:

- **Manual:** preencher e salvar.
- **Com PDF:** o backend lê o PDF, tenta identificar nome, e-mail e telefone e preenche o formulário; o usuário revisa antes de salvar. O PDF é opcional e uma falha na leitura nunca impede o cadastro manual.

## Tecnologias e versões

| Camada | Tecnologia |
|---|---|
| Frontend | Angular 19 (standalone components, reactive forms), TypeScript 5.6, Karma/Jasmine |
| Backend | ASP.NET Core Web API (.NET 8), Entity Framework Core 8, PdfPig 0.1.9 (leitura de PDF), xUnit |
| Banco | SQL Server 2022 (Docker, SQL Server local ou LocalDB) |

## Requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- Node.js 20 LTS ou superior (npm incluso) e Google Chrome (para os testes do Angular)
- Um SQL Server, por uma destas opções: Docker Desktop, SQL Server instalado (ou remoto) ou LocalDB (vem com o Visual Studio, apenas Windows)

## Estrutura

```
backend/src/PortalDeCurriculos.Api   API (controllers, modelos, serviços, EF Core)
backend/tests/PortalDeCurriculos.Tests Testes xUnit
backend/PortalDeCurriculos.sln         Solução do Visual Studio (API + testes)
frontend/                            Aplicação Angular
database/01_create_schema.sql        Script de criação do banco e da tabela
samples/curriculo-ficticio.pdf       Currículo fictício para testar a importação
docker-compose.yml                   SQL Server para desenvolvimento
```

## 1. Banco de dados

O script `database/01_create_schema.sql` cria o banco **PortalDeCurriculosDb** e a tabela `Candidatos`. Ele é idempotente: pode ser executado mais de uma vez sem erro. Escolha **uma** das opções abaixo.

### Opção A: Docker

Requer o Docker Desktop em execução (no Windows, com a virtualização habilitada).

1. Crie o arquivo `.env` com a senha do usuário `sa`. Use uma senha forte (mínimo de 8 caracteres, com maiúscula, minúscula, número e símbolo):

   ```
   # PowerShell
   Copy-Item .env.example .env
   # Linux/macOS
   cp .env.example .env
   ```

   Depois edite o `.env` e troque o valor de `SA_PASSWORD`.

2. Suba o SQL Server e aguarde cerca de 30 segundos até ele iniciar:

   ```
   docker compose up -d
   ```

3. Execute o script (os comandos funcionam no PowerShell e no bash; troque `SUA_SENHA` pela senha do `.env`):

   ```
   docker cp database/01_create_schema.sql curriculos-sql:/tmp/schema.sql
   docker exec curriculos-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "SUA_SENHA" -C -i /tmp/schema.sql
   ```

4. Para conferir, o banco deve aparecer na listagem:

   ```
   docker exec curriculos-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "SUA_SENHA" -C -Q "SELECT name FROM sys.databases"
   ```

Observações: `docker compose down` para o container e mantém os dados; `docker compose down -v` apaga os dados (será preciso rodar o script de novo). Se a porta 1433 estiver ocupada, troque `"1433:1433"` por `"14330:1433"` no `docker-compose.yml` e use `localhost,14330` na connection string.

### Opção B: LocalDB (Windows, com Visual Studio)

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i database/01_create_schema.sql
```

Ou abra o `database/01_create_schema.sql` no SSMS, conectado a `(localdb)\MSSQLLocalDB`, e execute.

### Opção C: SQL Server existente

```
sqlcmd -S <servidor> -U <usuario> -P <senha> -C -i database/01_create_schema.sql
```

Ou abra o script no SSMS/Azure Data Studio e execute.

## 2. Configurar a conexão (sem credenciais no repositório)

O `appsettings.json` traz apenas um valor de exemplo (`SUA_SENHA_AQUI`). Informe a conexão real por **user-secrets** (ficam fora do repositório):

```
dotnet user-secrets set "ConnectionStrings:Default" "<CONNECTION STRING>" --project backend/src/PortalDeCurriculos.Api
```

No Visual Studio, também é possível usar *botão direito no projeto > Manage User Secrets*.

Connection strings para cada opção do passo 1:

| Opção | Connection string |
|---|---|
| Docker | `Server=localhost,1433;Database=PortalDeCurriculosDb;User Id=sa;Password=<SUA_SENHA>;TrustServerCertificate=True` |
| LocalDB | `Server=(localdb)\MSSQLLocalDB;Database=PortalDeCurriculosDb;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server existente | `Server=<servidor>;Database=PortalDeCurriculosDb;User Id=<usuario>;Password=<senha>;TrustServerCertificate=True` |

Também é possível usar a variável de ambiente `ConnectionStrings__Default`.

## 3. Executar

### Pelo Visual Studio (backend)

1. *Arquivo > Abrir > Projeto/Solução* e escolha `backend/PortalDeCurriculos.sln`.
2. Configure a connection string (passo 2) com *botão direito em `PortalDeCurriculos.Api` > Gerenciar Segredos do Usuário*.
3. Selecione o perfil **http** e execute (F5). Os testes ficam em *Teste > Gerenciador de Testes*.

Se o Visual Studio não reconhecer a solução, gere uma nova com `dotnet new sln -n PortalDeCurriculos` dentro de `backend` e `dotnet sln add src/PortalDeCurriculos.Api tests/PortalDeCurriculos.Tests`.

### Pela linha de comando

Backend (http://localhost:5080):

```
dotnet run --project backend/src/PortalDeCurriculos.Api
```

Frontend (http://localhost:4200), em outro terminal:

```
cd frontend
npm install
npm start
```

O `ng serve` encaminha `/api` para o backend por meio do `frontend/proxy.conf.json`. Se mudar a porta do backend, ajuste esse arquivo.

Para testar a importação, use `samples/curriculo-ficticio.pdf` na tela **Novo cadastro**.

## 4. Testes

```
dotnet test backend/tests/PortalDeCurriculos.Tests
```

Não precisa de SQL Server (usa banco em memória).

```
cd frontend
npm test -- --watch=false --browsers=ChromeHeadless
```

## API

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/curriculos/extrair` | `multipart/form-data` com o campo `arquivo` (PDF de até 5 MB). Devolve `{ nomeCompleto, email, telefone }` (nulos se não encontrados). Não grava nada. |
| POST | `/api/candidatos` | Cria o candidato (valida os campos). |
| GET | `/api/candidatos?busca=` | Lista, mais recentes primeiro; `busca` filtra por nome, e-mail ou área. |
| GET | `/api/candidatos/{id}` | Detalhes. |

Erros seguem o formato *Problem Details* (`detail` traz a mensagem exibida ao usuário): 400 para arquivo ou campos inválidos, 404 para não encontrado e 422 para falha na leitura do PDF.

## Regras de validação (backend e frontend)

- Nome: obrigatório, até 150 caracteres. E-mail: obrigatório, formato válido, até 254.
- Telefone (opcional): de 8 a 30 caracteres entre números, espaços e `+ - . ( )`.
- Área/cargo: até 100. Resumo: até 4000.
- PDF: extensão `.pdf`, conteúdo iniciando com `%PDF-` e até 5 MB. O frontend valida antes de enviar; o backend valida sempre.

## Limitações conhecidas

- A identificação de nome e telefone usa heurísticas simples: pode errar ou não achar nada. Por isso o resultado é sempre revisável.
- PDFs escaneados (só imagem) não têm texto extraível; não há OCR.
- Não há autenticação nem verificação de e-mail duplicado.
