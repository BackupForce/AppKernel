using Application.Abstractions.Data;
using Application.Gaming.DrawGroups.Activate;
using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.Delete;
using Application.Gaming.DrawGroups.Draws.Add;
using Application.Gaming.DrawGroups.Draws.Remove;
using Application.Gaming.DrawGroups.End;
using Application.Gaming.DrawGroups.GetById;
using Application.Gaming.DrawGroups.List;
using Application.Gaming.DrawGroups.RemoteSearch;
using Application.Gaming.DrawGroups.Update;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups;

internal static class DrawGroupEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/drawgroups")
            .WithTags("Gaming.DrawGroups");

        //CRUD
        group.MapCreateDrawGroupEndpoint();
        group.MapGetDrawGroupEndpoint();
        group.MapGetDrawGroupsEndpoint();
        group.MapUpdateDrawGroupEndpoint();
        group.MapDeleteDrawGroupEndpoint();

        group.MapRemoteSearchDrawGroupsEndpoint();

        group.MapActivateDrawGroupEndpoint();
        group.MapEndDrawGroupEndpoint();

        //Draws
        group.MapAddDrawGroupDrawEndpoint();
        group.MapRemoveDrawGroupDrawEndpoint();
    }

}
