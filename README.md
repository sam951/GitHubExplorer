# GitHub Explorer

Applicazione .NET per cercare repository su GitHub e tenere una lista di preferiti con una nota personale. È divisa in due parti che comunicano solo via HTTP: una Web API in ASP.NET Core e un front-end in Blazor Server.

## Cosa fa

- Cerca tra i repository pubblici di GitHub, con risultati paginati e ordinabili per rilevanza, stelle, data di aggiornamento o fork.
- Accetta i qualificatori di ricerca di GitHub (per esempio `language:c#`, `stars:>1000`, `in:name`): la query viene passata a GitHub così com'è.
- Mostra i risultati come lista o come griglia, a scelta.
- Permette di salvare un repository tra i preferiti evitando i duplicati; nella lista di ricerca il pulsante indica quali repository sono già salvati.
- Consente di scrivere e modificare una nota su ogni preferito.
- Permette di eliminare un preferito, chiedendo conferma prima di procedere.

I preferiti sono salvati su MySQL.

## Stack

- .NET 10 e C#
- ASP.NET Core Web API (progetto `Api`)
- Blazor Server (progetto `Web`)
- MySQL 8, avviato in Docker
- Accesso ai dati in ADO.NET, senza ORM
- Test con MSTest

`MySqlConnector` fa solo da driver verso MySQL, non è un ORM. L'interfaccia è costruita interamente a mano, senza librerie di componenti.

## Struttura della soluzione

```
GitHubExplorer/
├─ GitHubExplorer.Api/         Web API: controller, sicurezza, accesso dati, client GitHub
├─ GitHubExplorer.Web/         Blazor Server: pagine, componenti, client verso l'API
├─ GitHubExplorer.Contracts/   DTO condivisi tra i due tier
├─ GitHubExplorer.Tests/       test (MSTest)
├─ db/schema.sql               schema MySQL, gestito a mano senza migration
├─ docker-compose.yml          MySQL in Docker
└─ .env.example                credenziali dev per docker-compose, da copiare in .env
```

`Web` e `Api` dipendono entrambi da `Contracts`, ma non si referenziano a vicenda: l'unico canale tra loro è HTTP.

## Come si esegue

Servono .NET 10 SDK, Git e MySQL. Per MySQL il modo più comodo è Docker Desktop, ma va bene anche un'installazione locale (vedi più avanti).

Clona il repository:

```bash
git clone https://github.com/sam951/GitHubExplorer.git
cd GitHubExplorer
```

Avvia il database. Copia le credenziali di sviluppo e fai partire il container; lo schema viene creato in automatico al primo avvio:

```bash
cp .env.example .env
docker compose up -d
```

Avvia i due progetti, l'API e il front-end. Da Visual Studio puoi impostare l'avvio multiplo; da riga di comando servono due terminali:

```bash
dotnet run --project GitHubExplorer.Api
```

```bash
dotnet run --project GitHubExplorer.Web
```

Apri il front-end su http://localhost:5056 (l'API risponde su http://localhost:5204). La configurazione di sviluppo, chiave API compresa, è già nei file `appsettings.json`, quindi non serve nessun passaggio manuale.

### Senza Docker, con un MySQL locale

Se hai già MySQL installato puoi saltare Docker. Salta il passo del `.env` e crea il database eseguendo lo script sul tuo MySQL:

```bash
mysql -u root -p < db/schema.sql
```

In PowerShell, che non supporta `<`, usa `Get-Content db\schema.sql | mysql -u root -p`. In alternativa apri `db/schema.sql` in MySQL Workbench e lancialo da lì.

Lo script crea il database, la tabella e l'utente applicativo (`ghexp_user`, con password `ghexp_dev_pass`), cioè le stesse credenziali che usa la connection string. Se il tuo MySQL non è sulla porta 3306 basta cambiare `Port=` nella connection string, in `appsettings.Development.json`. Poi riprendi dall'avvio dei progetti.

## Endpoint dell'API

Tutti gli endpoint richiedono l'header `X-Api-Key`.

| Metodo | Rotta | Descrizione | Risposte |
|---|---|---|---|
| GET | `/api/repositories?q={query}&page={n}&perPage={n}&sort={stars/forks/updated}` | ricerca su GitHub, paginata e ordinabile | 200, 400 se `q` è vuoto, 401 |
| GET | `/api/favorites` | lista dei preferiti | 200, 401 |
| POST | `/api/favorites` | aggiunge un preferito | 201, 400 se i dati non sono validi, 409 se duplicato, 401 |
| DELETE | `/api/favorites/{id}` | elimina un preferito | 204, 404, 401 |
| PUT | `/api/favorites/{id}/note` | aggiorna la nota | 204, 404, 401 |

La ricerca restituisce un risultato paginato, con `items` e `totalCount`. GitHub limita la ricerca a 1000 risultati e `perPage` a 100, quindi il totale che espongo è già limitato a 1000: in questo modo non si può navigare verso pagine che GitHub rifiuterebbe.

## Sicurezza

L'autenticazione è a chiave API su header (`X-Api-Key`). Un `AuthenticationHandler` valida la chiave e i controller sono protetti con `[Authorize]`; se la chiave manca o è sbagliata la risposta è 401. Il confronto della chiave è fatto a tempo costante con `CryptographicOperations.FixedTimeEquals`, per non lasciare spazio a un timing attack. La chiave di sviluppo (`dev-local-api-key-2026`) è committata solo per far girare il progetto senza configurazione, e non protegge niente di reale.

L'input viene validato con le DataAnnotations sul DTO di creazione. Grazie a `[ApiController]` un input non valido produce da solo una risposta 400 con ProblemDetails, senza controlli manuali nel controller.

La gestione degli errori è centralizzata in un `IExceptionHandler`, insieme a `AddProblemDetails()`, così le risposte d'errore seguono lo standard RFC 7807. Le eccezioni note vengono tradotte in modo specifico: un duplicato a database diventa un 409, GitHub irraggiungibile diventa un 502, tutto il resto un 500. Il corpo della risposta non espone dettagli interni; quelli finiscono nei log del server, correlabili tramite il `traceId`.

La configurazione di sviluppo sta nei `appsettings.json` per far partire subito il progetto. Gli override e i segreti veri stanno in `appsettings.Development.json` e in `.env`, entrambi fuori dal repository; in produzione i segreti andrebbero presi da variabili d'ambiente o da un secret manager. Il container MySQL è pubblicato solo su `127.0.0.1`, quindi non è mai esposto in rete, e le sue credenziali stanno in `.env`, che non è committato.

## Test

La suite è volutamente piccola: pochi test, ma sensati.

Gli unit test coprono la logica del `FavoritesController`: un duplicato non viene inserito e restituisce 409, un preferito nuovo restituisce 201. Al posto del repository reale usano un test double scritto a mano, uno stub con uno spy, invece di un framework di mocking. Sono veloci e non hanno dipendenze.

I test di integrazione invece esercitano i sistemi reali: il `GitHubClient` contro l'API vera di GitHub, per verificare il mapping dei dati, e il `FavoritesRepository` contro MySQL, per il ciclo di inserimento, lettura ed eliminazione. Richiedono rete e database attivi, e sono marcati con la categoria `Integration`.

```bash
dotnet test
```

Per lanciare solo i test che non richiedono rete o database:

```bash
dotnet test --filter "TestCategory!=Integration"
```

## Scelte progettuali

Non c'è un service layer. Le operazioni sui preferiti sono un CRUD sottile e l'unica logica vera è il controllo anti-duplicato, cioè un `if` più un vincolo di unicità: i controller chiamano direttamente il repository e il client. Un `FavoritesService` sarebbe stato solo un passacarte. Se domani arrivassero regole di business, lo introdurrei.

Il progetto `Contracts` per pochi DTO può sembrare eccessivo, ma serve a dichiarare il contratto tra i due tier in un punto solo, a non duplicare le classi e a tenere le dipendenze in una direzione sola: `Web` non deve conoscere `Api`.

Il controllo dei duplicati è a due livelli: un controllo applicativo (`ExistsAsync`), che dà un 409 pulito nel caso normale, e il vincolo di unicità sul `github_id`, come rete di sicurezza per la race condition tra il controllo e l'inserimento.

Paginazione e ordinamento sono lato server: `page`, `perPage` e `sort` attraversano le tier fino a GitHub. Il tetto di 1000 risultati, che è un limite dell'API di GitHub, lo applico nel client GitHub, così il front-end non deve saperne niente.

Il dialog di conferma è un componente riutilizzabile scritto a mano. Espone un metodo `ShowAsync` che ritorna un `Task<bool>`, appoggiandosi a un `TaskCompletionSource`, così il chiamante aspetta la scelta dell'utente con una riga sola, senza gestire stato sparso per la pagina.

Non ho usato Clean Architecture, CQRS o DDD: a questa scala il codice di supporto supererebbe quello che fa il lavoro, e sarebbe over-engineering.

Con Blazor Server la chiave API resta sul server: il `DelegatingHandler` che la aggiunge alle richieste gira lato server e non raggiunge mai il browser.

L'interfaccia è tutta scritta a mano: i componenti (`RepositoryCard`, `Spinner`, `Toast`, `ConfirmDialog`), un `ToastService`, e lo stile in CSS.

L'aspetto è volutamente sobrio. Ho preferito un'interfaccia pulita e curata quanto basta, senza esagerare con il design: per un'applicazione del genere qualcosa di troppo vistoso avrebbe rischiato di sembrare fuori misura. L'obiettivo era che fosse chiara e comoda da usare, non che fosse l'estetica a prendersi la scena.

## Possibili migliorie

- Gestire il rate limit di GitHub in modo dedicato, con un 429 o un 503 e un messaggio, invece del 502 generico.
- Spostare i segreti su user-secrets o un secret manager, e aggiungere health check e logging strutturato.
- Ricordare la preferenza di vista, lista o griglia, tra una sessione e l'altra.
