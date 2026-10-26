# Gestão de Projetos — Construtora

Aplicação para cadastrar projetos de uma construtora, com status e risco, e vincular a eles funcionários aleatórios obtidos da API pública [randomuser.me](https://randomuser.me/).

**Stack:** ASP.NET Core (.NET 10) · EF Core + PostgreSQL · React + TypeScript (Vite, Mantine, TanStack Query) · xUnit + Testcontainers · Docker Compose

---

## Furos de análise e premissas assumidas

O enunciado deixa alguns pontos em aberto. Abaixo está cada um deles, a decisão tomada e o motivo.

| # | Ponto em aberto | Decisão implementada | Motivo |
|---|---|---|---|
| 1 | *"Se um projeto **foi mudado** para iniciado, em andamento ou encerrado não pode mais ser excluído"*: vale o status **atual** ou o **histórico**? | **Histórico.** Se o projeto passou por um desses status em algum momento, ele nunca mais pode ser excluído, mesmo que depois mude para *cancelado* ou *planejado*. Isso fica gravado no campo `exclusao_bloqueada`. | É a leitura literal de "foi mudado". Olhar só o status atual permitiria burlar a regra: mudar para *cancelado* e excluir. |
| 2 | Existe um fluxo obrigatório entre os status? | **Não.** As transições são livres. A única garantia é que o projeto tem sempre um status, e apenas um. | O enunciado não define um fluxo. Criar uma máquina de estados seria inventar uma regra de negócio. |
| 3 | Um projeto pode ser criado já como *iniciado*? | **Sim.** E, nesse caso, ele já nasce sem poder ser excluído. | O status é um campo do cadastro, e a regra do item 1 se aplica a partir da criação. |
| 4 | Como os funcionários aleatórios são vinculados? | **Sorteio automático.** No detalhe do projeto, o usuário escolhe uma quantidade (de 1 a 10) e clica em "Sortear e vincular". A API busca essa quantidade no randomuser, grava os funcionários e os vincula. | É o que o enunciado pede ("vincular funcionários aleatórios"), com poucos cliques. |
| 5 | De onde vem o **Responsável**? | É um **Funcionário** do mesmo cadastro (chave estrangeira). No formulário, o usuário busca entre os já importados ou clica em "Sortear" para importar um novo do randomuser. | O enunciado indica a API de pessoas. Usar a mesma entidade evita duplicar dados. |
| 6 | Um funcionário pode estar em mais de um projeto? | **Sim** (relação N:N). No mesmo projeto, porém, não pode aparecer duas vezes. | Nada impede no enunciado, e é o comportamento comum numa construtora. |
| 7 | Dá para remover um vínculo? | **Sim**, há a opção de desvincular um funcionário. | Completa o ciclo "vincular", e um sorteio errado pode ser corrigido. |
| 8 | A **data de término** é obrigatória? | **Não.** Quando informada, precisa ser maior ou igual à data de início. | No início de um projeto, a data de término muitas vezes ainda não é conhecida. |
| 9 | A **descrição** é obrigatória? | **Sim**, com no máximo 2000 caracteres. O nome tem no máximo 150. | É um dos campos listados para o projeto. |
| 10 | A exclusão é lógica ou física? | **Física.** Os vínculos do projeto são removidos em cascata, e os funcionários continuam cadastrados. | O enunciado fala em "exclusão" e não menciona auditoria. |
| 11 | O mesmo funcionário do randomuser pode ser importado duas vezes? | **Não.** O `login.uuid` do randomuser é gravado com índice único, e um funcionário que já existe é reaproveitado. | Mantém a integridade da base. |
| 12 | Os funcionários do randomuser são de qualquer nacionalidade? | Por padrão, `nat=br`, para gerar nomes brasileiros. É configurável em `RandomUser:Nacionalidade`, e deixar o valor vazio libera qualquer nacionalidade. | Combina melhor com o contexto de uma construtora brasileira. |

**Fora do escopo** (não foi pedido): autenticação e autorização, auditoria ou histórico de status, CQRS/mediator e pipeline de CI/CD.

---

## Como executar

### Opção 1: Docker (recomendado)

Pré-requisito: Docker. Não é preciso ter .NET nem Node instalados.

```bash
docker compose up -d --build
```

| Serviço | URL |
|---|---|
| Aplicação (React) | http://localhost:3000 |
| API (Swagger) | http://localhost:5090/swagger |
| Health check | http://localhost:5090/health |
| PostgreSQL | `localhost:5432` (usuário e senha `postgres`, banco `desbravador_projetos`) |

As migrations são aplicadas automaticamente quando a API sobe. Para parar e apagar os dados:

```bash
docker compose down -v
```

### Opção 2: Desenvolvimento local

Pré-requisitos: .NET SDK 10, Node 20+ e Docker (só para o banco).

```bash
# 1. Banco de dados
docker compose up -d db

# 2. API → http://localhost:5090/swagger
dotnet run --project backend/src/Desbravador.Projetos.Api

# 3. Front-end → http://localhost:5173 (o Vite encaminha /api para a API)
cd frontend
npm install
npm run dev
```

---

## Testes

```bash
dotnet test backend/Desbravador.Projetos.slnx
```

- **Unitários** (`tests/Desbravador.Projetos.UnitTests`, 55 testes):
  - regras de domínio: status único, bloqueio de exclusão pelo histórico, datas, vínculos duplicados;
  - serviços de aplicação, com NSubstitute;
  - client do randomuser, com um `HttpMessageHandler` falso: monta a query, mapeia os campos e trata falhas.
- **Integração** (`tests/Desbravador.Projetos.IntegrationTests`, 9 testes):
  - sobem a API real com `WebApplicationFactory` contra um **PostgreSQL real em container** (Testcontainers);
  - cobrem CRUD completo, exclusão bloqueada (422), vincular e desvincular, filtros e paginação, validação (400) e randomuser indisponível (503);
  - **exigem o Docker em execução**;
  - só a API externa é trocada por um fake, para os testes serem determinísticos.

Checagens do front-end:

```bash
cd frontend
npm run build   # checagem de tipos + build
npm run lint
```

---

## Arquitetura

Clean Architecture enxuta. As dependências apontam para dentro: `Api → Infrastructure → Application → Domain`.

```
backend/
├── src/
│   ├── Desbravador.Projetos.Domain          # Entidades, enums e regras de negócio (sem dependências)
│   ├── Desbravador.Projetos.Application     # Serviços, DTOs, validações (FluentValidation) e interfaces
│   ├── Desbravador.Projetos.Infrastructure  # EF Core/PostgreSQL, repositórios, client randomuser.me
│   └── Desbravador.Projetos.Api             # Controllers, tratamento de erros, Swagger, composição
└── tests/
    ├── Desbravador.Projetos.UnitTests
    └── Desbravador.Projetos.IntegrationTests
frontend/
└── src/
    ├── api/         # Cliente HTTP, endpoints e hooks do React Query
    ├── components/  # Formulário, seleção de responsável, badges, estados (carregando, erro, vazio)
    ├── domain/      # Rótulos e cores de status e risco, formatação
    └── pages/       # Lista, cadastro e edição, detalhe
```

**Decisões principais**

- **As regras ficam no domínio.** A entidade `Projeto` encapsula:
  - a troca de status;
  - o bloqueio de exclusão (`ExclusaoBloqueada`, que nunca volta a ser falso);
  - a validação de datas;
  - o vínculo de funcionários.

  Os setters são privados, então não há como deixar uma entidade em estado inválido.
- **Os status não são cadastráveis.** São `enum`s gravados como texto no banco e trafegados como texto no JSON (`"EmAndamento"`).
- **Os erros seguem um padrão.** Um `IExceptionHandler` converte exceções em `ProblemDetails` (RFC 9457):

  | Situação | Resposta |
  |---|---|
  | Validação | 400, com erros por campo |
  | Recurso não encontrado | 404 |
  | Regra de negócio | 422 |
  | randomuser fora do ar | 503 |

  O front exibe esses erros no próprio formulário ou como notificação.
- **A listagem usa uma projeção no banco.** Ela vai direto para o DTO de resumo, com a contagem de funcionários calculada em SQL, sem carregar as entidades.

### Integração com randomuser.me

- **Uma requisição por sorteio:** `?results=N` traz N funcionários de uma vez, em vez de N chamadas.
- **Payload mínimo:** `&inc=login,name,email&noinfo` pede só os campos usados.
- **Resiliência:** `Microsoft.Extensions.Http.Resilience` (Polly) adiciona retry com backoff, circuit breaker e timeout. Se a API cair, o usuário recebe uma mensagem clara (503), e nada é gravado pela metade.
- **Persistência local:** os funcionários importados ficam no PostgreSQL, então listagens e detalhes não dependem da API externa.

### Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/projetos?busca=&status=&risco=&pagina=&tamanhoPagina=` | Lista com filtros e paginação |
| GET | `/api/projetos/{id}` | Detalhe com os funcionários vinculados |
| POST | `/api/projetos` | Cadastra |
| PUT | `/api/projetos/{id}` | Altera (inclusive o status) |
| DELETE | `/api/projetos/{id}` | Exclui (422 se já passou por iniciado, em andamento ou encerrado) |
| POST | `/api/projetos/{id}/funcionarios/aleatorios` | Sorteia `{ "quantidade": N }` funcionários e vincula |
| DELETE | `/api/projetos/{id}/funcionarios/{funcionarioId}` | Desvincula |
| GET | `/api/funcionarios?busca=&limite=` | Funcionários já importados (para escolher o responsável) |
| POST | `/api/funcionarios/aleatorios` | Importa `{ "quantidade": N }` funcionários do randomuser |

A documentação interativa completa está no Swagger.

### Experiência do usuário

- **Listagem:**
  - busca com debounce e filtros de status e risco guardados na URL, então sobrevivem ao recarregar e ao voltar do detalhe;
  - paginação;
  - tabela no desktop e cards no celular.
- **Formulário:**
  - validação no cliente, espelhando as regras do servidor, e exibição dos erros do servidor por campo;
  - aviso quando o status escolhido vai bloquear a exclusão.
- **Exclusão:**
  - pede confirmação;
  - o botão fica desabilitado, com um tooltip explicando o motivo, quando a regra impede.
- **Feedback:** notificações de sucesso e erro, além de estados de carregando, vazio e erro com "tentar novamente".
