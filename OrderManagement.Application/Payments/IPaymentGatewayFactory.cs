namespace OrderManagement.Application.Payments;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(string provider);
}


