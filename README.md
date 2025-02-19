# ABP Framework

![build and test](https://img.shields.io/github/actions/workflow/status/abpframework/abp/build-and-test.yml?branch=dev&style=flat-square) 🔹 [![codecov](https://codecov.io/gh/abpframework/abp/branch/dev/graph/badge.svg?token=jUKLCxa6HF)](https://codecov.io/gh/abpframework/abp) 🔹 [![NuGet](https://img.shields.io/nuget/v/Volo.Abp.Core.svg?style=flat-square)](https://www.nuget.org/packages/Volo.Abp.Core) 🔹 [![NuGet (with prereleases)](https://img.shields.io/nuget/vpre/Volo.Abp.Core.svg?style=flat-square)](https://www.nuget.org/packages/Volo.Abp.Core) 🔹 [![MyGet (nightly builds)](https://img.shields.io/myget/abp-nightly/vpre/Volo.Abp.svg?style=flat-square)](https://docs.abp.io/en/abp/latest/Nightly-Builds) 🔹 
[![NuGet Download](https://img.shields.io/nuget/dt/Volo.Abp.Core.svg?style=flat-square)](https://www.nuget.org/packages/Volo.Abp.Core) 🔹 [![Code of Conduct](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](https://github.com/abpframework/abp/blob/dev/CODE_OF_CONDUCT.md) 🔹 [![CLA Signed](https://cla-assistant.io/readme/badge/abpframework/abp)](https://cla-assistant.io/abpframework/abp) 🔹 [![Discord Shield](https://discord.com/api/guilds/951497912645476422/widget.png?style=shield)](https://discord.gg/abp)

ABP Framework is a complete **infrastructure** based on **ASP.NET Core** that creates **modern web applications** and **APIs** by following the software development **best practices** and the **latest technologies**.

[![ABP Platform](https://github.com/abpframework/abp/assets/9526587/47531496-4088-406d-9c69-63cb0ffec2ba)](https://abp.io)


## Getting Started

- [Quick Start](https://docs.abp.io/en/abp/latest/Tutorials/Todo/Index) is a single-part, quick-start tutorial to build a simple application with the ABP Framework. Start with this tutorial if you want to understand how ABP works quickly.
- [Getting Started guide](https://docs.abp.io/en/abp/latest/Getting-Started) can be used to create and run ABP-based solutions with different options and details.
- [Web Application Development Tutorial](https://docs.abp.io/en/abp/latest/Tutorials/Part-1) is a complete tutorial on developing a full-stack web application with all aspects of a real-life solution.

### Quick Start

Install the ABP CLI:

````bash
> dotnet tool install -g Volo.Abp.Cli
````

Create a new solution:

````bash
> abp new BookStore -u mvc -d ef
````

> See the [CLI documentation](https://docs.abp.io/en/abp/latest/CLI) for all available options.



### UI Framework Options

<img width="500" src="docs/en/images/ui-options.png">



### Database Provider Options

<img width="500" src="docs/en/images/db-options.png">



## What ABP Provides?

ABP provides a **full stack developer experience**.



### Architecture

<img src="docs/en/images/ddd-microservice-simple.png">

ABP offers a complete, **modular** and **layered** software architecture based on **[Domain Driven Design](https://docs.abp.io/en/abp/latest/Domain-Driven-Design)** principles and patterns. It also provides the necessary infrastructure and guidance to [implement this architecture](https://docs.abp.io/en/abp/latest/Domain-Driven-Design-Implementation-Guide).

ABP Framework is suitable for **[microservice solutions](https://docs.abp.io/en/abp/latest/Microservice-Architecture)** as well as monolithic applications.



### Infrastructure

There are a lot of features provided by the ABP Framework to achieve real-world scenarios easier, like [Event Bus](https://docs.abp.io/en/abp/latest/Event-Bus), [Background Job System](https://docs.abp.io/en/abp/latest/Background-Jobs), [Audit Logging](https://docs.abp.io/en/abp/latest/Audit-Logging), [BLOB Storing](https://docs.abp.io/en/abp/latest/Blob-Storing), [Data Seeding](https://docs.abp.io/en/abp/latest/Data-Seeding), [Data Filtering](https://docs.abp.io/en/abp/latest/Data-Filtering), etc.



### Cross-Cutting Concerns

ABP also simplifies (and even automates wherever possible) cross-cutting concerns and common non-functional requirements like [Exception Handling](https://docs.abp.io/en/abp/latest/Exception-Handling), [Validation](https://docs.abp.io/en/abp/latest/Validation), [Authorization](https://docs.abp.io/en/abp/latest/Authorization), [Localization](https://docs.abp.io/en/abp/latest/Localization), [Caching](https://docs.abp.io/en/abp/latest/Caching), [Dependency Injection](https://docs.abp.io/en/abp/latest/Dependency-Injection), [Setting Management](https://docs.abp.io/en/abp/latest/Settings), etc.



### Application Modules

ABP is a modular framework and the Application Modules provide **pre-built application functionalities**;

- [**Account**](https://docs.abp.io/en/abp/latest/Modules/Account): Provides UI for the account management and allows user to login/register to the application.
- **[Identity](https://docs.abp.io/en/abp/latest/Modules/Identity)**: Manages organization units, roles, users and their permissions based on the Microsoft Identity library.
- [**OpenIddict**](https://docs.abp.io/en/abp/latest/Modules/OpenIddict): Integrates to OpenIddict.
- [**Tenant Management**](https://docs.abp.io/en/abp/latest/Modules/Tenant-Management): Manages tenants for a [multi-tenant](https://docs.abp.io/en/abp/latest/Multi-Tenancy) (SaaS) application.

See the [Application Modules](https://docs.abp.io/en/abp/latest/Modules/Index) document for all pre-built modules.



### Startup Templates

The [Startup templates](https://docs.abp.io/en/abp/latest/Startup-Templates/Index) are pre-built Visual Studio solution templates. You can create your own solution based on these templates to **immediately start your development**.



## Mastering ABP Framework Book

This book will help you to gain a complete understanding of the ABP Framework and modern web application development techniques. It is written by the creator and team lead of the ABP Framework. You can buy from [Amazon](https://www.amazon.com/gp/product/B097Z2DM8Q) or [Packt Publishing](https://www.packtpub.com/product/mastering-abp-framework/9781801079242). Find further info about the book at [abp.io/books/mastering-abp-framework](https://abp.io/books/mastering-abp-framework).

![book-mastering-abp-framework](docs/en/images/book-mastering-abp-framework.png)



## The Community

### ABP Community Web Site

The [ABP Community](https://community.abp.io/) is a website to publish **articles** and share **knowledge** about the ABP Framework. You can also create content for the community!

### Blog

Follow the [ABP Blog](https://blog.abp.io/) to learn the latest happenings in the ABP Framework.

### Samples

See the [sample projects](https://docs.abp.io/en/abp/latest/Samples/Index) built with the ABP Framework.

### Want to Contribute?

ABP is a community-driven open-source project. See [the contribution guide](https://docs.abp.io/en/abp/latest/Contribution/Index) if you want to participate in this project.



## Official Links

* [Home Website](https://abp.io)
  * [Get Started](https://abp.io/get-started)
  * [Features](https://abp.io/features)
* [Documents](https://docs.abp.io/)
* [Samples](https://docs.abp.io/en/abp/latest/Samples/Index)
* [Blog](https://blog.abp.io/)
* [Community](https://community.abp.io/)
* [Stackoverflow](https://stackoverflow.com/questions/tagged/abp)
* [Twitter](https://twitter.com/abpframework)



## Support ABP

GitHub repository stars are an important indicator of popularity and the size of the community. If you like ABP Framework, support us by clicking the star :star: on the repository.



## Discord Server

We have a Discord server where you can chat with other ABP users. Share your ideas, report technical issues, showcase your creations, share the tips that worked for you and catch up with the latest news and announcements about ABP Framework. Join 👉 https://discord.gg/abp.

