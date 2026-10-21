using System;

public class GameEventSelectionOption
{
    public GameEventDefinition Definition { get; }
    public Action OnSelected { get; }

    public GameEventSelectionOption(GameEventDefinition definition, Action onSelected)
    {
        Definition = definition;
        OnSelected = onSelected;
    }
}
