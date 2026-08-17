namespace GummyDynasty.Cognition
{
    public static class HierarchyIds
    {
        public const string World = "world";
        public const string FactionRed = "faction-red";
        public const string ArmyWest = "army-west";
        public const string FormationRed = "formation-red";
    }

    public static class HierarchyRoles
    {
        public const string Objective = "objective";
        public const string Threat = "threat";
        public const string Pain = "pain";
        public const string Stand = "stand";
        public const string Congestion = "congestion";
        public const string Cost = "cost";
        public const string Pressure = "pressure";
        public const string Wall = "wall";
        public const string Breach = "breach";
        public const string Hold = "hold";
    }

    /// <summary>Installs WORLD → FACTION → ARMY → FORMATION. Actors attach later.</summary>
    public static class BattleHierarchy
    {
        public static void Install(BeliefField field)
        {
            field.AddNode(HierarchyIds.World, null, NodeKind.World);
            field.EnsureRole(HierarchyIds.World, HierarchyRoles.Pressure, RoleMode.Dissipative, 0.2f, ReduceOp.Mean);
            field.EnsureRole(HierarchyIds.World, HierarchyRoles.Cost, RoleMode.Dissipative, 0.15f, ReduceOp.Mean);

            field.AddNode(HierarchyIds.FactionRed, HierarchyIds.World, NodeKind.Faction);
            EnsureCommand(field, HierarchyIds.FactionRed);
            EnsureReports(field, HierarchyIds.FactionRed);

            field.AddNode(HierarchyIds.ArmyWest, HierarchyIds.FactionRed, NodeKind.Army);
            EnsureCommand(field, HierarchyIds.ArmyWest);
            EnsureReports(field, HierarchyIds.ArmyWest);

            field.AddNode(HierarchyIds.FormationRed, HierarchyIds.ArmyWest, NodeKind.Formation);
            EnsureCommand(field, HierarchyIds.FormationRed);
            EnsureReports(field, HierarchyIds.FormationRed);
        }

        public static void IssueWestOrder(BeliefField field)
        {
            field.Observe(new Observation(HierarchyIds.FactionRed, HierarchyRoles.Objective, 1f, 1f));
            field.Observe(new Observation(HierarchyIds.ArmyWest, HierarchyRoles.Objective, 1f, 1f));
            field.Observe(new Observation(HierarchyIds.FormationRed, HierarchyRoles.Objective, 1f, 1f));
            ObserveHold(field, 0f);
        }

        public static void IssueHoldOrder(BeliefField field)
        {
            field.Observe(new Observation(HierarchyIds.FactionRed, HierarchyRoles.Objective, 1f, 1f));
            field.Observe(new Observation(HierarchyIds.ArmyWest, HierarchyRoles.Objective, 1f, 1f));
            field.Observe(new Observation(HierarchyIds.FormationRed, HierarchyRoles.Objective, 1f, 1f));
            ObserveHold(field, 1f);
        }

        static void ObserveHold(BeliefField field, float value)
        {
            field.Observe(new Observation(HierarchyIds.FactionRed, HierarchyRoles.Hold, value, 1f));
            field.Observe(new Observation(HierarchyIds.ArmyWest, HierarchyRoles.Hold, value, 1f));
            field.Observe(new Observation(HierarchyIds.FormationRed, HierarchyRoles.Hold, value, 1f));
        }

        public static void ObserveRoad(BeliefField field, float wallAhead, float hole)
        {
            field.Observe(new Observation(HierarchyIds.FormationRed, HierarchyRoles.Wall, wallAhead, 1f));
            field.Observe(new Observation(HierarchyIds.FormationRed, HierarchyRoles.Breach, hole, 1f));
        }

        public static void AttachActor(BeliefField field, string actorId)
        {
            field.AddNode(actorId, HierarchyIds.FormationRed, NodeKind.Actor);
            field.EnsureRole(actorId, HierarchyRoles.Threat, RoleMode.Dissipative, 0.5f);
            field.EnsureRole(actorId, HierarchyRoles.Pain, RoleMode.Dissipative, 0.25f);
            field.EnsureRole(actorId, HierarchyRoles.Stand, RoleMode.Dissipative, 0.4f);
            field.EnsureRole(actorId, HierarchyRoles.Objective, RoleMode.Unitary);
            field.EnsureRole(actorId, HierarchyRoles.Congestion, RoleMode.Dissipative, 0.45f, ReduceOp.Mean);
            field.EnsureRole(actorId, HierarchyRoles.Cost, RoleMode.Dissipative, 0.3f, ReduceOp.Mean);
        }

        static void EnsureCommand(BeliefField field, string nodeId)
        {
            field.EnsureRole(nodeId, HierarchyRoles.Objective, RoleMode.Unitary);
            field.EnsureRole(nodeId, HierarchyRoles.Hold, RoleMode.Unitary);
        }

        static void EnsureReports(BeliefField field, string nodeId)
        {
            field.EnsureRole(nodeId, HierarchyRoles.Congestion, RoleMode.Dissipative, 0.35f, ReduceOp.Mean);
            field.EnsureRole(nodeId, HierarchyRoles.Cost, RoleMode.Dissipative, 0.25f, ReduceOp.Mean);
            field.EnsureRole(nodeId, HierarchyRoles.Threat, RoleMode.Dissipative, 0.4f, ReduceOp.Max);
            field.EnsureRole(nodeId, HierarchyRoles.Pain, RoleMode.Dissipative, 0.3f, ReduceOp.Mean);
            field.EnsureRole(nodeId, HierarchyRoles.Wall, RoleMode.Dissipative, 0.2f);
            field.EnsureRole(nodeId, HierarchyRoles.Breach, RoleMode.Dissipative, 0.2f);
        }
    }
}
