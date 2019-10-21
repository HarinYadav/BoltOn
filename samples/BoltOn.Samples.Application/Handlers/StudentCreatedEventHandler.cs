﻿using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class StudentCreatedEvent : CqrsEvent
	{
		public Guid StudentId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string StudentType { get; set; }
	}

	public class StudentCreatedEventHandler : IRequestAsyncHandler<StudentCreatedEvent>
	{
		private readonly IRepository<StudentFlattened> _repository;

		public StudentCreatedEventHandler(IRepository<StudentFlattened> repository)
		{
			_repository = repository;
		}

		public async Task HandleAsync(StudentCreatedEvent request, CancellationToken cancellationToken)
		{
			var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);

			if (student == null)
				await _repository.AddAsync(new StudentFlattened(request), cancellationToken);
		}
	}
}