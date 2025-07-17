namespace WorkflowEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            var workflowDefs = new Dictionary<string, WorkflowDefinition>();
            var wfInstances = new Dictionary<string, WorkflowInstance>();

            app.MapGet("/", ()=> "Workflow Engine API running!");

            app.MapPost("/workflows", (WorkflowDefinition wfDef)=> {
                if (workflowDefs.ContainsKey(wfDef.Id)) return Results.BadRequest("Workflow already exists.");
                if (wfDef.States.Count(s => s.IsInitial) != 1) return Results.BadRequest("Must have exactly one initial state.");
                workflowDefs[wfDef.Id] = wfDef; return Results.Ok(wfDef);
            });

            app.MapGet("/workflows", ()=> 
                workflowDefs.Values
            );

            app.MapPost("/instances/{definitionId}", (string definitionId) =>
            {
                if (!workflowDefs.TryGetValue(definitionId, out var wfDef))
                    return Results.NotFound("Definition not found.");

                var initial = wfDef.States.First(s => s.IsInitial);

                var newInst = new WorkflowInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowDefinitionId=definitionId,
                    CurrentStateId=initial.Id
                };

                wfInstances[newInst.Id]=newInst;
                return Results.Ok(newInst);
            });

            app.MapPost("/instances/{instanceId}/actions/{actionId}", (string instanceId, string actionId) =>
            {
            if (!wfInstances.TryGetValue(instanceId, out var inst)) return Results.NotFound("Instance not found.");

                var wfDef = workflowDefs[inst.WorkflowDefinitionId];
                var action = wfDef.Actions.FirstOrDefault(a => a.Id == actionId);

                if (action == null || !action.Enabled) return Results.BadRequest("Action invalid or disabled.");
                if (!action.FromStates.Contains(inst.CurrentStateId)) return Results.BadRequest("Action not valid from current state.");

                var currState = wfDef.States.FirstOrDefault(s => s.Id == inst.CurrentStateId);

                if (currState != null && currState.IsFinal)
                    return Results.BadRequest("Instance is already in a final state.");

                var targetState = wfDef.States.FirstOrDefault(s => s.Id == action.ToState && s.Enabled);
                if (targetState == null) return Results.BadRequest("Target state invalid or disabled.");

                inst.CurrentStateId = targetState.Id;
                inst.History.Add(actionId);

                return Results.Ok(inst);
            });

            app.MapGet("/instances/{instanceId}", 
                (string instanceId) =>
            {
                if (!wfInstances.TryGetValue(instanceId, out var inst))
                    return Results.NotFound();
                return Results.Ok(inst);
            });

            app.Run();
        }
    }


    public class State {
        public string Id {get;set;}
        public bool IsInitial{get; set;}
        public bool IsFinal {get;set;}
        public bool Enabled {get; set;}
    }

    public class Action
    {
    public string Id { get;set; }
        public List<string> FromStates{get; set;}
        public string ToState {get; set;}
        public bool Enabled{get; set;}
    }

    public class WorkflowDefinition
    {
        public string Id {get; set;}
        public List<State> States{get; set;}
        public List<Action> Actions {get; set;}
    }

    public class WorkflowInstance
    {
        public string Id{get;set;}
        public string WorkflowDefinitionId{get; set;}
        public string CurrentStateId {get; set;}
        public List<string> History{get;set;} = new();
    }
}
