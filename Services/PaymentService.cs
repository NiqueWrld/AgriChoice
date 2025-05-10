using Braintree;

namespace AgriChoice.Services
{
    public class PaymentService
    {
        private readonly BraintreeGateway _gateway;

        public PaymentService()
        {
            _gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                MerchantId = "dxk9tgr3636qzbcs",
                PublicKey = "5vqh6y6jgdnsrx3z",
                PrivateKey = "0f9ca18c85e71b65b81fba01a9a51811"
            };
        }

        public IBraintreeGateway GetGateway() => _gateway;

        public async Task<string> CreateTransactionAsync(decimal amount, string paymentMethodNonce)
        {
            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodNonce = paymentMethodNonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            Result<Transaction> result = await _gateway.Transaction.SaleAsync(request);

            if (result.IsSuccess())
            {
                return result.Target.Id;
            }
            else
            {
                var message = result.Message ?? "Braintree error occurred.";
                throw new Exception($"Transaction failed: {message}");
            }
        }

        public async Task<string> GenerateClientTokenAsync()
        {
            return await _gateway.ClientToken.GenerateAsync();
        }
    }
}