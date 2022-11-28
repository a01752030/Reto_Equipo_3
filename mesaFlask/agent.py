from mesa import Agent

class Car(Agent):
    """
    Agent that moves randomly.
    Attributes:
        unique_id: Agent's ID 
        direction: Randomly chosen direction chosen from one of eight directions
    """
    def __init__(self, unique_id, model, pos):
        """
        Creates a new random agent.
        Args:
            unique_id: The agent's ID
            model: Model reference for the agent
        """
        super().__init__(unique_id, model)
        self.pos = pos
        self.lastDirection = None
        self.direction = None

    def sense(self):
        Tracks = []
        coin = [1,2]
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        neighbors = self.model.grid.iter_neighbors(self.pos,moore = False, include_center=True)
        for neighbor in neighbors: 
            if isinstance(neighbor,Road) and neighbor.direction != self.direction:
                Tracks.append(neighbor)
            
        if len(Tracks) == 0 or self.direction == None:
            for neighbor in cellmates:
                if isinstance(neighbor, Traffic_Light):
                    if neighbor.state == True:
                        self.move()
                    else:
                        pass
                elif isinstance(neighbor, Road):
                    self.direction = neighbor.direction
                    self.move()
            
        elif len(Tracks) == 1:
            throw = self.random.choice(coin)
            if throw == 1:
                for neighbor in cellmates:
                    if isinstance(neighbor, Traffic_Light):
                        if neighbor.state == True:
                            self.move()
                        else:
                            pass
                    elif isinstance(neighbor, Road):
                        self.direction = neighbor.direction
                        self.move()
            else:
                self.MoveToPlace(Tracks[0].pos)

        else:
            for neighbor in cellmates:
                if isinstance(neighbor, Traffic_Light):
                    if neighbor.state == True:
                        self.move()
                    else:
                        pass
                elif isinstance(neighbor, Road):
                    self.direction = neighbor.direction
                    self.move()


    def move(self):
        """ 
        Determines if the agent can move in the direction that was chosen
        """
        new_position = (0, 0)
        if self.direction == "Left":
            new_position = (self.pos[0] - 1, self.pos[1])
        elif self.direction == "Right":
            new_position = (self.pos[0] + 1, self.pos[1])
        elif self.direction == "Down":
            new_position = (self.pos[0], self.pos[1] - 1)
        elif self.direction == "Up":
            new_position = (self.pos[0], self.pos[1] + 1)
        self.model.grid.move_agent(self, new_position)
        
    def MoveToPlace(self, new_position):
        self.model.grid.move_agent(self, new_position)

    def step(self):
        """ 
        Determines the new direction it will take, and then moves
        """
        self.sense()

class Traffic_Light(Agent):
    """
    Traffic light. Where the traffic lights are in the grid.
    """
    def __init__(self, unique_id, model, state = False, timeToChange = 10):
        super().__init__(unique_id, model)
        """
        Creates a new Traffic light.
        Args:
            unique_id: The agent's ID
            model: Model reference for the agent
            state: Whether the traffic light is green or red
            timeToChange: After how many step should the traffic light change color 
        """
        self.state = state
        self.timeToChange = timeToChange
        self.rotate = state

    def step(self):
        """ 
        To change the state (green or red) of the traffic light in case you consider the time to change of each traffic light.
        """
        pass

class Destination(Agent):
    """
    Destination agent. Where each car should go.
    """
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)

    def step(self):
        pass

class Obstacle(Agent):
    """
    Obstacle agent. Just to add obstacles to the grid.
    """
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)

    def step(self):
        pass

class Road(Agent):
    """
    Road agent. Determines where the cars can move, and in which direction.
    """
    def __init__(self, unique_id, model, pos, direction):
        """
        Creates a new road.
        Args:
            unique_id: The agent's ID
            model: Model reference for the agent
            direction: Direction where the cars can move
        """
        super().__init__(unique_id, model)
        self.pos = pos
        self.direction = direction

    def step(self):
        print(self.direction)
