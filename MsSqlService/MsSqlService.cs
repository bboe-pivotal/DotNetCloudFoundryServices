﻿using System.Linq;
using CloudFoundryServiceBroker.Interfaces;
using CloudFoundryServiceBroker.Interfaces.Entities;
using System.ComponentModel.Composition;
using System.Configuration;

namespace MsSqlService
{
    [Export(typeof(ICloudFoundryService))]
    public class MsSqlService : ICloudFoundryService
    {
        private readonly MsSqlServiceInfo _serviceInfo;
        private readonly MsSqlProvider _msSqlProvider;

        public MsSqlService()
        {
            _serviceInfo = new MsSqlServiceInfo();
            _msSqlProvider = new MsSqlProvider();
        }

        public ICloudFoundryServiceInfo ServiceInfo
        {
            get { return _serviceInfo; }
        }

        public ProvisioningServiceResponse ProvisionInstance(ProvisioningServiceRequest request)
        {
            var dashboardUrl = ConfigurationManager.AppSettings[Constants.AppKeys.DashboardUrl];

            var plan = _serviceInfo.SqlPlans.First(x=>x.Id==request.PlanId);
            _msSqlProvider.CreateDatabase(request.ServiceInstanceId, plan);
            return new ProvisioningServiceResponse { Url = dashboardUrl };
        }

        public CreateBindingResponse CreateBinding(CreateBindingRequest request)
        {
            var logUrl = ConfigurationManager.AppSettings[Constants.AppKeys.LogUrl];

            var plan = _serviceInfo.SqlPlans.First(x => x.Id == request.PlanId);
            var credentials = _msSqlProvider.CreateUserForDatabase(request.ServiceInstanceId, request.BindingInstanceId, plan);
            return new CreateBindingResponse { Credentials = credentials, LogUrl = logUrl };
        }

        public RemoveBindingResponse RemoveBinding(RemoveBindingRequest request)
        {
            _msSqlProvider.RemoveUserFromDatabase(request.ServiceInstanceId, request.BindingInstanceId);
            return new RemoveBindingResponse();
        }

        public RemoveBindingResponse RemoveInstance(RemoveInstanceRequest request)
        {
            _msSqlProvider.DropDatabase(request.ServiceInstanceId);
            return new RemoveBindingResponse();
        }
    }
}
