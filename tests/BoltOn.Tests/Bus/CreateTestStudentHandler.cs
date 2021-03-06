﻿using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Logging;

namespace BoltOn.Tests.Bus
{
	public class CreateTestStudent : IRequest
	{
		public string FirstName { get; set; }
	}

	public class CreateTestStudentHandler : IHandler<CreateTestStudent>
    {
        private readonly IBoltOnLogger<CreateTestStudentHandler> _logger;

        public CreateTestStudentHandler(IBoltOnLogger<CreateTestStudentHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(CreateTestStudent request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(CreateTestStudentHandler)} invoked");
            await Task.FromResult("testing");
        }
    }
}
