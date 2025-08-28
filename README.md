



# Virto Commerce Experience API (xApi) Module

[![CI status](https://github.com/VirtoCommerce/vc-module-x-api/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-x-api/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-x-api&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-x-api) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-x-api&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-x-api) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-x-api&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-x-api) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-x-api&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-x-api)

The Experience API (xAPI) module is the core business API component, central to handling general API functions and integrations within the VirtoCommerce platform. As part of our transition to a more composable architecture, the XAPI module is designed to provide a robust and flexible foundation for business GraphQL / API interactions, ensuring seamless integration and efficient API management.

## Architecture
The primary function of the Experience API (xAPI) module is to isolate high-performance ecommerce business logic from client applications and data providers. This approach involves the following architectural components:

* **Client Applications**: Front-end interfaces, such as web or mobile applications, interact with the xAPI to request and manipulate ecommerce data.
* **xAPI Modules**: Acts as an intermediary layer that encapsulates and exposes business logic through a set of well-defined APIs. It ensures that the business rules and processes are consistently applied across different clients.
* **Data Providers**: Backend systems, including databases and third-party services, supply the necessary data to the xAPI. The module manages data retrieval, aggregation, and transformation, ensuring optimal performance and scalability.
  
By decoupling business logic from client applications and data sources, the xAPI module promotes a modular architecture that enhances maintainability, scalability, and flexibility. This design allows developers to update or replace components independently without disrupting the overall system, fostering a more resilient and adaptable ecommerce platform.

## Key Features
- [xAPI](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/): Core business API module.
- [xCatalog](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/Catalog/overview/): Manages catalog-related operations.
- [xCart](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/Cart/overview/): Handles cart-related functionalities.
- [xOrders](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/Order/overview/): Manages order processing.
- [xProfile](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/Profile/overview/): Handles My Customer and My Organization functionalities.
and more ...

## Modularity
To better support the growing business API with GraphQL, we are transitioning from a single VirtoCommerce.ExperienceApi module to a more composable architecture consisting of multiple new modules. This change aims to simplify business API development and the release cycle. 

At this moment, The following new modules provide common XAPI functionality that is used from the Virto Commerce Frontend:
* VirtoCommerce.Xapi: Core business API module.
* VirtoCommerce.XCart: Handles cart-related functionalities.
* VirtoCommerce.XCatalog: Manages catalog-related operations.
* VirtoCommerce.XCMS: Content management system integration.
* VirtoCommerce.XOrder: Manages order processing.
* VirtoCommerce.ProfileExperienceApiModule
* VirtoCommerce.PushMessages
* VirtoCommerce.Quote

Other modules that add additional Xapi Scenarios:
* VirtoCommerce.Contracts
* VirtoCommerce.MarketingExperienceApi
* VirtoCommerce.Quote
* VirtoCommerce.CustomerReviews
* VirtoCommerce.Skyflow
* VirtoCommerce.TaskManagement
* VirtoCommerce.FileExperienceApi
* VirtoCommerce.WhiteLabeling

## Documentation

* [Experience API Documentation](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/)
* [Getting started](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/getting-started/)
* [How to use GraphiQL](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/graphiql/)
* [How to use Postman](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/postman/)
* [How to extend](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/x-api-extensions/)
* [Virto Commerce Frontend architecture](https://docs.virtocommerce.org/storefront/developer-guide/architecture/)

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-inventory/releases/latest)

## License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
