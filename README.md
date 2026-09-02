# GitHub Explorer

Applicazione full-stack .NET per **cercare repository su GitHub** e gestire una lista di **preferiti** con nota personale. Il progetto è diviso in due tier che comunicano solo via HTTP: una **Web API** (ASP.NET Core) e un front-end **Blazor Server**.

---

## Cosa fa

- Ricerca di repository pubblici su GitHub, con **paginazione** dei risultati e **ordinamento** (rilevanza, stelle, aggiornamento, fork).
- Supporto ai **qualificatori di ricerca** di GitHub (es. `language:c#`, `stars:>1000`, `in:name`): la query viene passata così com'è all'API di GitHub.
- Vista risultati a **lista o griglia**, a scelta.
- Salvataggio di un repository tra i preferiti, con controllo anti-duplicato; nella lista di ricerca il pulsante segnala i repository **già presenti** nei preferiti.
- Nota personale su ogni preferito (aggiungibile/modificabile).
- Eliminazione di un preferito, con **dialog di conferma** custom.

I preferiti sono persistiti su **MySQL**.

---

## Stack

- **.NET 10** / C#
- **ASP.NET Core Web API** (controller) — tier `Api`
- **Blazor Server** (interattività globale) — tier `Web`
- **MySQL 8** in Docker
- **ADO.NET puro** per l'accesso ai dati (nessun ORM)
- **MSTest** (runner VSTest) per i test

### Vincoli richiesti e come sono stati onorati

- **Niente ORM / Entity Framework** → accesso ai dati scritto a mano in **ADO.NET**. `MySqlConnector` è usato solo come **driver** del database (non è un ORM: senza un provider non è possibile dialogare con MySQL).
- **Niente librerie esterne** (es. suite di componenti UI) → tutta l'interfaccia, il sistema di **toast**, il **dialog di conferma** e i **test double** sono costruiti a mano. MSTest è tooling di test, non una dipendenza runtime dell'applicazione.

---

## Struttura della soluzione

```
GitHubExplorer/
├─ GitHubExplorer.Api/         # Web API: controller, sicurezza, accesso dati, client GitHub
├─ GitHubExplorer.Web/         # Blazor Server: pagine, componenti, client tipizzato verso l'API
├─ GitHubExplorer.Contracts/   # DTO condivisi (contratto di wire tra i due tier)
├─ GitHubExplorer.Tests/       # Test (MSTest)
├─ db/schema.sql               # Schema MySQL (nessuna migration: gestito a mano)
├─ docker-compose.yml          # MySQL in Docker
└─ .env.example                # Credenziali dev per docker-compose (da copiare in .env)
```

`Web` e `Api` dipendono entrambi da `Contracts`, ma **non si referenziano tra loro**: comunicano solo via HTTP.

---

## Come si esegue

**Prerequisiti:** .NET 10 SDK, Git, e MySQL — via **Docker Desktop** (consigliato) oppure un'installazione **locale** (vedi la variante in fondo alla sezione).

1. **Clona** il repository:
   ```bash
   git clone https://github.com/sam951/GitHubExplorer.git
   cd GitHubExplorer
   ```

2. **Database** — copia le credenziali dev e avvia MySQL:
   ```bash
   cp .env.example .env
   docker compose up -d
   ```
   Lo schema viene creato automaticamente al primo avvio del container.

3. **Applicazione** — avvia **Api** e **Web** (in Visual Studio: progetti di avvio multipli; da CLI, in due terminali):
   ```bash
   dotnet run --project GitHubExplorer.Api
   ```
   ```bash
   dotnet run --project GitHubExplorer.Web
   ```

4. Apri il front-end su **http://localhost:5056** (l'API risponde su **http://localhost:5204**).

La config dev (chiave API compresa) è già nei file `appsettings.json`: l'app funziona senza passaggi manuali.

### In alternativa: senza Docker (MySQL locale)

Se hai già MySQL installato, Docker non serve: salta il passo 2 (niente `.env`) e crea il database eseguendo lo script sul tuo MySQL.

- Da riga di comando, nella cartella del progetto:
  ```bash
  mysql -u root -p < db/schema.sql
  ```
  In PowerShell (che non supporta `<`): `Get-Content db\schema.sql | mysql -u root -p`.
- Oppure apri `db/schema.sql` in MySQL Workbench ed eseguilo.

Lo script crea database, tabella e utente applicativo (`ghexp_user` / `ghexp_dev_pass`) — le stesse credenziali della connection string. Se il tuo MySQL non è su `localhost:3306`, adegua `Port=` nella connection string (in `appsettings.Development.json`, override locale fuori dal repo). Poi prosegui dal passo 3.

---

## Endpoint API

Tutti gli endpoint richiedono l'header `X-Api-Key`.

| Metodo | Rotta | Descrizione | Esiti |
|---|---|---|---|
| GET | `/api/repositories?q={query}&page={n}&perPage={n}&sort={stars/forks/updated}` | Ricerca su GitHub (paginata, ordinabile) | 200 · 400 (q vuoto) · 401 |
| GET | `/api/favorites` | Lista preferiti | 200 · 401 |
| POST | `/api/favorites` | Aggiunge un preferito | 201 · 400 (dati non validi) · 409 (duplicato) · 401 |
| DELETE | `/api/favorites/{id}` | Elimina un preferito | 204 · 404 · 401 |
| PUT | `/api/favorites/{id}/note` | Aggiorna la nota | 204 · 404 · 401 |

La ricerca restituisce un risultato **paginato** (`items` + `totalCount`). GitHub limita la ricerca a **1000 risultati** e `perPage` a 100: il totale esposto è già cappato a 1000, così la navigazione non genera pagine non valide.

---

## Sicurezza

- **API Key su header** (`X-Api-Key`): un `AuthenticationHandler` valida la chiave e `[Authorize]` protegge i controller. Chiave assente o errata → **401**. Il confronto è a **tempo costante** (`CryptographicOperations.FixedTimeEquals`) per non esporre un timing side-channel.
  - Chiave dev committata (`dev-local-api-key-2026`) per far girare il progetto out-of-the-box: non protegge nulla di reale.
- **Validazione dell'input**: il DTO di creazione preferito usa DataAnnotations; grazie a `[ApiController]` un input non valido produce automaticamente un **400** con ProblemDetails di validazione, senza controlli manuali nel controller.
- **Gestione errori centralizzata**: un `IExceptionHandler` + `AddProblemDetails()` producono risposte **RFC 7807** coerenti. Le eccezioni note sono mappate (duplicato DB → 409, GitHub irraggiungibile → 502), il resto → 500. Il corpo **non espone** dettagli interni; il dettaglio finisce nei log server-side, correlabile via `traceId`.
- **Configurazione**: la config dev è nei `appsettings.json` per l'esecuzione immediata; `appsettings.Development.json` e `.env` (entrambi fuori dal repo) servono per override e segreti reali. In produzione i segreti andrebbero da variabili d'ambiente / secret manager.
- **Docker**: MySQL è pubblicato solo su `127.0.0.1` (mai esposto in rete); le credenziali del container vivono in `.env` (non committato).

---

## Test

Piramide volutamente snella (i test sono *sensati*, non tanti per fare numero):

- **Unità** — logica del `FavoritesController` (anti-duplicato → 409 senza inserire, nuovo → 201). Usa un **test double scritto a mano** (stub + spy) al posto del repository, coerente col vincolo "niente librerie" (nessun mocking framework). Veloce e senza dipendenze.
- **Integrazione** — `GitHubClient` contro l'API reale di GitHub (mapping dei dati) e `FavoritesRepository` contro MySQL reale (ciclo CRUD). Richiedono rete e database attivi (categoria `Integration`).

```bash
dotnet test
```

Per escludere i test che richiedono rete/DB:

```bash
dotnet test --filter "TestCategory!=Integration"
```

---

## Scelte progettuali e trade-off

- **Nessun service layer.** Le operazioni sui preferiti sono CRUD sottili; l'unica logica reale è il controllo anti-duplicato (un `if` + un vincolo `UNIQUE`). Un `FavoritesService` sarebbe stato un pass-through vuoto (anemic service): i controller chiamano direttamente repository e client. Se dovessero arrivare regole di business, si introdurrebbe.
- **Progetto `Contracts`.** Un progetto per pochi DTO può sembrare cerimonia, ma dichiara l'intento (il *contratto* tra i tier), evita duplicazione e tiene le dipendenze a senso unico (`Web` non referenzia `Api`).
- **Dedup a due livelli.** Pre-check applicativo (`ExistsAsync`) per il 409 pulito nel caso normale, e vincolo `UNIQUE(github_id)` come rete di sicurezza ultima (race condition).
- **Paginazione e ordinamento server-side.** `page`, `perPage` e `sort` attraversano le tier fino a GitHub; il tetto di 1000 risultati (limite duro dell'API di GitHub) è applicato nel client GitHub, così il front-end non deve conoscerne i dettagli.
- **Dialog di conferma riutilizzabile.** Componente costruito a mano che espone un metodo `ShowAsync` con esito `Task<bool>` (pattern basato su `TaskCompletionSource`): il chiamante attende la scelta dell'utente con una singola riga, senza gestire stato sparso.
- **Niente Clean Architecture / CQRS / DDD.** A questa scala il plumbing supererebbe la logica: sarebbe over-engineering.
- **Blazor Server (non WASM).** La chiave API resta lato server: il `DelegatingHandler` che la inietta gira sul server, non raggiunge mai il browser.
- **UI e toast a mano.** Componenti (`RepositoryCard`, `Spinner`, `Toaster`, `ConfirmDialog`) e un `ToastService` scoped costruiti da zero; styling in CSS scritto a mano (con CSS isolation dove sensato).

---

## Possibili migliorie future

- Rate-limit di GitHub gestito in modo dedicato (429/503 con messaggio) invece di 502 generico.
- Segreti da user-secrets / secret manager; health check e logging strutturato.
- Persistenza della preferenza di vista (lista/griglia) tra le sessioni.
