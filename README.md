# Repo for my site

> basically the second iteration of my site's backend, this time using an ORM:

- c#
- entity core instead of Dapper
- sql server instead of postgres

---

## Still TODO

- [ ] optimize the responses according to the [rfc](https://datatracker.ietf.org/doc/html/rfc7807)
- [ ] Comments
- [ ] likes
- [ ] User
  - [ ] likes (collections)
  - [ ] posts
  - [ ] comments
  - [ ] refresh token(s) 
    - [token rfc](https://datatracker.ietf.org/doc/html/rfc7519)
    - [OAuth docs](https://backstage.forgerock.com/docs/am/7.1/oauth2-guide/oauth2-refresh-tokens.html)
    - [rfc](https://datatracker.ietf.org/doc/html/rfc6749#section-5.1)
- [X] Account Management
  - [X] Password reset
  - [X] Email Verification
- [ ] create a post
  - [ ] image upload for post creation
- [ ] think of more stuff !
- [ ] tests
  - [ ] integration
  - [ ] unit
- [ ]  social login
  - [repo example](https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers/blob/dev/samples/Mvc.Client/Controllers/AuthenticationController.cs)
  - [microsoft docs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/other-logins?view=aspnetcore-7.0)
  - [package](https://www.nuget.org/packages/AspNet.Security.OAuth.GitHub) AspNet.Security.OAuth.GitHub
  - [github docs](https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps)
  - [github app](https://github.com/settings/applications/2342165)
