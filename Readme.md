# Rocket Surgeons Guild Conventions [![Build status](https://ci.appveyor.com/api/projects/status/kc7f36cbo4bo1nij/branch/master?svg=true)](https://ci.appveyor.com/project/david-driscoll/conventions) [![codecov](https://codecov.io/gh/RocketSurgeonsGuild/Conventions/branch/master/graph/badge.svg)](https://codecov.io/gh/RocketSurgeonsGuild/Conventions)

This is a set of libraries to help in creating code conventions, and allow for code discovery without doing a full assembly scan.

* Builders
  Are a simple class and interface for creating a builder interface with the ability to exit and return to a parent builder
* Conveitons
  Defines an attribute `[Convention(typeof(T))]` that is used as an assembly attribute.  This allows consuming applications to only need to scan the assembly attributes to get a hold of the convention.  `IConvention<>` then defines a `Register` method that can takes an arbitrary context object.  Consumes can then setup this context to allow for attribute scanning.


## More info to come...
