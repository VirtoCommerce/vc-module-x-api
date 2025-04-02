//Call this to register our module to main application
var moduleName = "virtoCommerce.xApi";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['platformWebApp.devToolsList',
            function (devToolsList) {
                devToolsList.add({
                    name: 'GraphQL',
                    url: '/ui/graphiql'
                });
            }]);
