# Step 7 — Categories (GraphQL Queries + Mutations)

First real data feature. Full CRUD over GraphQL, user-scoped. Introduces mutations,
input types, and a service layer returning `Result`.

> Status: ✅ complete. Full CRUD over GraphQL, user-scoped; cross-user isolation
> verified (User2 gets NOT_FOUND on User1's rows).

---

## Operations

- **Query** `myCategories` — the current user's categories.
- **Mutations** `createCategory`, `updateCategory`, `deleteCategory`.

All scoped to `ICurrentUser.Id` — you only ever see/touch your own.

## GraphQL concepts

- **Mutation type** — writes live on a separate `Mutation` class (queries =
  reads on `Query`). Registered via `.AddMutationType<Mutation>()`.
- **Input types** — structured mutation args. A C# `record` (e.g.
  `CreateCategoryInput(string Name)`) maps to a GraphQL `input` automatically.
- **Resolvers stay thin** — they call `CategoryService` and return the value;
  business logic + ownership live in the service (mirrors the Auth slice).

Layering: `resolver → CategoryService (Result) → AppDbContext`.

## Ownership enforcement (security)

Every operation filters `UserId == currentUser.Id`. Update/delete load the entity
**scoped to the user**; if not found for that user → `NotFound` (don't reveal it
exists for someone else). Stops ID-guessing across users.

## Result → GraphQL errors

REST mapped `Result` → HTTP status. GraphQL uses a structured `errors` array. For
now a failed `Result` throws a `GraphQLException` with the error code/message (HC
renders it as a GraphQL error). Formalized in Step 9.

## Decisions

- **Unique category name per user** — DB unique index on `(UserId, Name)`. Create/
  update return `Conflict` on a duplicate.
- **Delete when in use** — FK rule **RESTRICT** (set in Step 8 when Expenses exist):
  can't delete a category that has expenses.

## Chunks

- [x] 7a — `Category` entity + `CategoryConfiguration` (FK to User, unique `(UserId,Name)`)
- [x] 7b — `AddCategories` migration; applied; `categories` table verified in Postgres
- [x] 7c — `CategoryDto` + `CreateCategoryInput` / `UpdateCategoryInput`
- [x] 7d — `ICategoryService` + `CategoryService` (user-scoped CRUD, `Result`) + `AddCategories`
- [x] 7e — `CategoryQueries`/`CategoryMutations` type extensions + root `Mutation` + register
- [x] 7f — built + tested over GraphQL (incl. cross-user isolation)

## Verified behavior

| Operation | Result |
|-----------|--------|
| createCategory | returns category |
| duplicate name | error `CONFLICT` |
| myCategories (owner) | lists own |
| myCategories (other user) | `[]` — isolated |
| update/delete other user's category | `NOTFOUND` (no cross-user access) |
| update/delete own | works |
| empty name | `VALIDATION` |
| unauthenticated | `AUTH_NOT_AUTHENTICATED` |

Request collection: `http/categories.http` (uses GraphQL variables).

## GraphQL type extensions (modular resolvers)

Features contribute resolvers via `[ExtendObjectType<Query>]` /
`[ExtendObjectType<Mutation>]` instead of editing central `Query`/`Mutation`
classes. Root `Mutation` is an empty marker; `CategoryQueries`/`CategoryMutations`
add fields, registered with `.AddTypeExtension<T>()`. Keeps each feature
self-contained.

## Result → GraphQL error bridge

`ResultGraphQLExtensions.ValueOrThrow()` / `EnsureSuccess()` throw a
`GraphQLException` (code = ErrorType, message) on failure → HC renders it in the
response `errors` array.
