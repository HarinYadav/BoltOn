﻿using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Data;

namespace BoltOn.Cqrs
{
	public abstract class BaseCqrsEntity : BaseEntity<Guid>
	{
		private HashSet<ICqrsEvent> _eventsToBeProcessed = new HashSet<ICqrsEvent>();

		private HashSet<ICqrsEvent> _processedEvents = new HashSet<ICqrsEvent>();

		public virtual IEnumerable<ICqrsEvent> EventsToBeProcessed
		{
			get => _eventsToBeProcessed;
			internal set => _eventsToBeProcessed = value == null
				? new HashSet<ICqrsEvent>()
				: new HashSet<ICqrsEvent>(value);
		}

		public virtual IEnumerable<ICqrsEvent> ProcessedEvents
		{
			get => _processedEvents;
			internal set => _processedEvents = value == null
				? new HashSet<ICqrsEvent>()
				: new HashSet<ICqrsEvent>(value);
		}

		protected bool RaiseEvent<TEvent>(TEvent @event)
			where TEvent : ICqrsEvent
		{
			if (_eventsToBeProcessed.Any(c => c.Id == @event.Id))
				return false;

			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			@event.SourceId = Id;
			@event.SourceTypeName = GetType().AssemblyQualifiedName;
			// events with CreatedDate == null are filtered in the repository. this is used 
			// to differentiate events that were added in the current request and the existing events
			@event.CreatedDate = null;
			_eventsToBeProcessed.Add(@event);
			return true;
		}

		protected bool ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)
			where TEvent : ICqrsEvent
		{
			if (_processedEvents.Any(c => c.Id == @event.Id))
				return false;

			action(@event);
			@event.DestinationId = Id;
			@event.DestinationTypeName = GetType().AssemblyQualifiedName;
			// events with ProcessedDate == null are filtered in the repository. this is used 
			// to differentiate events that were added in the current request and the existing events
			@event.ProcessedDate = null;
			_processedEvents.Add(@event);
			return true;
		}

		internal void RemoveEventToBeProcessed<TEvent>(TEvent @event)
			where TEvent : ICqrsEvent
		{
			_eventsToBeProcessed.Remove(@event);
		}

		internal void RemoveProcessedEvent<TEvent>(TEvent @event)
			where TEvent : ICqrsEvent
		{
			_processedEvents.Remove(@event);
		}
	}
}
