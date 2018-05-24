using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using GladNet;
using Module = Autofac.Module;

namespace FreecraftCore
{
	/// <summary>
	/// Base class for all handler registerations.
	/// Implementers should inherit from this Type which allows the child type to be used as a handler registeration module
	/// that can register all handlers defined in the assembly.
	/// </summary>
	/// <typeparam name="TIncomingPayloadType"></typeparam>
	/// <typeparam name="TOutgoingPayloadType"></typeparam>
	/// <typeparam name="TPeerContextType"></typeparam>
	public abstract class PayloadHandlerRegisterationModule<TIncomingPayloadType, TOutgoingPayloadType, TPeerContextType> : Module
		where TPeerContextType : IPeerMessageContext<TOutgoingPayloadType> 
		where TOutgoingPayloadType : class
		where TIncomingPayloadType : class
	{
		/// <inheritdoc />
		protected override void Load(ContainerBuilder builder)
		{
			IEnumerable<Type> handlerTypes = LoadHandlerTypes();

			//Registers each type.
			foreach(Type t in handlerTypes)
				builder.RegisterType(t)
					.AsSelf()
					.SingleInstance();

			foreach(Type t in handlerTypes)
			{
				Type concretePayloadType = t.GetTypeInfo()
					.ImplementedInterfaces
					.First(i => i.GetTypeInfo().IsGenericType && i.GetTypeInfo().GetGenericTypeDefinition() == typeof(IPeerPayloadSpecificMessageHandler<,,>))
					.GetGenericArguments()
					.First();

				Type tryHandlerType = typeof(TrySemanticsBasedOnTypePeerMessageHandler<,,,>)
					.MakeGenericType(typeof(TIncomingPayloadType), typeof(TOutgoingPayloadType), concretePayloadType, typeof(TPeerContextType));

				builder.Register(context =>
					{
						object handler = context.Resolve(t);

						return Activator.CreateInstance(tryHandlerType, handler);
					})
					.As(typeof(IPeerMessageHandler<TIncomingPayloadType, TOutgoingPayloadType, TPeerContextType>))
					.SingleInstance();
			}
		}

		private IReadOnlyCollection<Type> LoadHandlerTypes()
		{
			//This loads all the handlers on the current assembly that the child Type is defined within.
			return GetType().GetTypeInfo()
				.Assembly
				.GetTypes()
				.Where(t => t.GetInterfaces().Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IPeerPayloadSpecificMessageHandler<,,>) && i.GenericTypeArguments.Contains(typeof(TPeerContextType)))) //must check context type now
				.ToArray();
		}
	}
}
