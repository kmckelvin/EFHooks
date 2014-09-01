## 1.0.2.0

* Upgraded Entity Framework to v4.2.0.0

## 1.0.3.0

* Stopped checking validation on Unchanged entities as this prevents stubs from being used (for things like associations)

## 1.0.4.0

* Upgraded Entity Framework to v5.0.0
* New .NET 4.0 and .NET 4.5 assemblies

## 1.0.5.0

* Merged branches of Visoft and Robert Sirre, adding metadata validation upon save (4.5 feature)

## 1.0.5.1

* Bugfix related to metadata validation (causing some PreActionHooks not to to be called)

## 1.0.5.2

* Added feature suggested by kirkchen : https://github.com/Atrejoe/EFHooks/pull/1

## 1.0.5.3

* Also updated 4.5 version

## 1.0.5.4

* Bugfix > Corrected failing unit test
* Feature > Allowed IHook to target multiple Entity States

## 2.0.0.0

* **Breaking Changes**
* Upgraded Entity Framework to v6.0.1 - EF6 changed namespaces (e.g. `System.Data` to `System.Data.Entity`) around. Because of this, v2.0+ won't work with EF5. Please continue to use an earlier version for your EF5 projects.

## 2.1.0.0

* Added savechanges async
