var moduleName = "virtoCommerce.xApi";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['$injector',
            function ($injector) {
                if ($injector.has('platformWebApp.developerTools')) {
                    var developerTools = $injector.get('platformWebApp.developerTools');
                    developerTools.add({
                        name: 'GraphQL',
                        url: '/ui/graphiql',
                    });
                }
            }]);
