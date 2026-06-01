# Glamour Perfumaria e Acessórios

E-commerce completo de perfumes e acessórios, com loja virtual e painel administrativo. Identidade visual sofisticada em **preto + dourado**, com temas que se adaptam por gênero (rosé para feminino, azul-aço para masculino).

> Loja física em **Cacimba de Dentro / PB**. Pagamento na entrega ou retirada — sem gateway online.

---

## Sumário

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Funcionalidades](#funcionalidades)
- [Regras de negócio](#regras-de-negócio)
- [Como executar](#como-executar)
- [Acesso](#acesso)
- [Usabilidade](#usabilidade)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Deploy](#deploy)

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core MVC (.NET 10, C#) |
| ORM | Entity Framework Core |
| Banco de dados | PostgreSQL 16 |
| Cache / sessão | Redis 7 (carrinho e favoritos) |
| Autenticação | ASP.NET Core Identity + Login com Google (OAuth 2.0) |
| Validação | FluentValidation |
| Logs | Serilog (console + arquivo) |
| Front-end | Razor Views + CSS próprio + Alpine.js |
| Proxy / TLS | Nginx |
| Containers | Docker + Docker Compose |
| CI/CD | GitHub Actions |

**Outros recursos:** PWA instalável, banner de consentimento de cookies (LGPD), compressão Brotli/Gzip, rate limiting, headers de segurança e integração com a API ViaCEP.

---

## Arquitetura

Arquitetura em camadas (Clean Architecture) sobre o padrão MVC:

```
Glamour.Web            → Controllers, Views (Razor), ModelBinding (camada MVC)
Glamour.Application    → Serviços, DTOs, validações, casos de uso
Glamour.Domain         → Entidades, enums, regras de negócio, interfaces
Glamour.Infrastructure → EF Core, repositórios, Identity, integrações (Redis)
Glamour.Shared         → Utilidades compartilhadas
```

Validações de negócio centralizadas via Notification Pattern, mantendo os controllers limpos.

---

## Funcionalidades

### Loja (cliente)

- **Catálogo** com filtros por busca, categoria, gênero, ordenação e paginação
- **Temas por gênero** — a paleta muda ao navegar (Feminino rosé, Masculino azul-aço, Unissex dourado)
- **Busca com autocomplete** no cabeçalho
- **Página de produto** com galeria de imagens, avaliações, produtos relacionados e formas de pagamento
- **Promoções** em destaque na home, com selo de desconto e preço "de/por"
- **Carrinho** persistente (Redis), com barra de progresso de frete grátis
- **Checkout** com entrega ou retirada na loja, cálculo de frete por localização e autopreenchimento de endereço via CEP
- **Favoritos** (wishlist)
- **Quiz olfativo** — recomenda perfumes a partir do perfil do cliente
- **Programa de fidelidade** — pontos a cada compra
- **Newsletter** e **notificação de volta ao estoque**
- **Avaliações** de produtos pelos clientes

### Conta do cliente

- Cadastro e login por e-mail/senha ou **conta Google**
- Perfil com dados pessoais e alteração de senha
- Gestão de endereços (com endereço principal)
- Histórico de pedidos com **linha do tempo do status**

### Painel administrativo

- **Dashboard** com vendas do dia e pedidos pendentes
- **Produtos** — CRUD, upload de imagens, controle de estoque e gestão de promoções (com prévia do desconto)
- **Categorias** — CRUD e ordenação
- **Pedidos** — atualização de status, código de rastreio e **dados de contato do cliente** (telefone + WhatsApp)
- **Cupons** — criação de descontos (percentual ou valor fixo)
- **Avaliações** — moderação (aprovar / remover)
- **Clientes** — listagem, histórico e bloqueio
- **Relatórios** — exportação de vendas e produtos em CSV

---

## Regras de negócio

**Frete (a partir da loja em Cacimba de Dentro / PB):**

| Situação | Frete |
|---|---|
| Cidade atendida, pedido **≥ R$ 150** | Grátis |
| Cidade atendida, pedido **< R$ 150** | R$ 4,99 |
| Retirada na loja | Grátis |
| Fora da área de entrega | Cliente entra em contato com a loja (WhatsApp) |

**Pagamento:** realizado no momento da entrega ou retirada. O cliente informa o método no checkout — dinheiro, cartão de crédito, cartão de débito ou PIX. Não há processamento de pagamento online.

**Promoções:** ao definir um preço promocional no admin, o produto exibe "de/por" e entra na vitrine de promoções. Ao remover o valor promocional, volta ao preço normal automaticamente.

---

## Como executar

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (para PostgreSQL e Redis)

### Passos

```bash
# 1. Subir banco de dados e cache
docker compose up -d postgres redis

# 2. Executar a aplicação
dotnet run --project src/Glamour.Web
```

A aplicação aplica as migrations e popula dados de demonstração automaticamente na primeira execução.

Acesse: **http://localhost:5039**

### Executar tudo via Docker

```bash
docker compose up -d --build
```

Sobe aplicação + PostgreSQL + Redis + Nginx. Acesse via **http://localhost**.

---

## Acesso

**Administrador (criado automaticamente):**

```
E-mail: admin@glamour.com
Senha:  Admin@123456
```

Ao fazer login, o administrador é direcionado direto ao dashboard (`/admin`). Clientes vão para a página inicial.

### Login com Google (opcional)

O login social fica ativo apenas quando as credenciais estão configuradas. Configure via variáveis de ambiente ou user-secrets:

```bash
dotnet user-secrets set "Authentication:Google:ClientSecret" "SEU_SECRET" --project src/Glamour.Web
```

E o `ClientId` em `appsettings.json` (seção `Authentication:Google`). No Google Cloud Console, autorize a URI de redirecionamento `http://localhost:5039/signin-google` (e a URL de produção no deploy).

---

## Usabilidade

### Fluxo do cliente

1. Navega pelo **catálogo** (ou usa a busca / quiz olfativo)
2. Abre um **produto**, escolhe a quantidade e adiciona ao **carrinho**
3. No **checkout**, escolhe entrega (com endereço) ou retirada na loja, e a forma de pagamento
4. Confirma o pedido — o pagamento é feito na entrega/retirada
5. Acompanha o status em **Minha Conta → Meus Pedidos**

### Fluxo do administrador

1. Acessa `/admin` e faz login
2. Cadastra **categorias** e **produtos** (com imagens e promoções)
3. Recebe os **pedidos** e atualiza o status (e código de rastreio, quando entrega)
4. Usa o contato do cliente (WhatsApp) para combinar a entrega/retirada
5. Cria **cupons**, modera **avaliações** e acompanha **relatórios**

---

## Estrutura do projeto

```
glamour/
├── docker-compose.yml          # App + PostgreSQL + Redis + Nginx
├── nginx/                      # Configuração do proxy reverso
├── .github/workflows/          # Pipeline CI/CD
└── src/
    ├── Glamour.Web/            # MVC: controllers, views, wwwroot
    ├── Glamour.Application/    # Serviços, DTOs, validators
    ├── Glamour.Domain/         # Entidades, enums, interfaces
    ├── Glamour.Infrastructure/ # EF Core, repositórios, Identity, Redis
    └── Glamour.Shared/         # Utilidades
```

---

## Deploy

O projeto está pronto para deploy em **VPS** (ex.: Hostinger KVM, DigitalOcean) via Docker Compose, ou em plataformas PaaS (Railway). O `docker-compose.yml` já inclui aplicação, banco, cache e Nginx.

Antes do deploy em produção:

- Definir segredos via variáveis de ambiente (senha do banco, secret do Google)
- Configurar domínio e SSL (Let's Encrypt) no Nginx
- Adicionar a URI de redirecionamento de produção no Google Cloud Console
- Agendar backup automático do PostgreSQL
