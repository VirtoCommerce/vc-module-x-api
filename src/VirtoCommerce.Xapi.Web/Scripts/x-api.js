//Call this to register our module to main application
var moduleName = "virtoCommerce.xApi";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['$injector',
            function ($injector) {
                if ($injector.has('platformWebApp.developerToolsList')) {
                    var devToolsList = $injector.get('platformWebApp.developerToolsList');
                    devToolsList.add({
                        name: 'GraphQL',
                        url: '/ui/graphiql'
                    });
                }
            }]);
