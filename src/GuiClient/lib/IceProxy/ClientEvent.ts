import { EventEmitter } from "events";
import { TreeDiagram } from "../../Ice/Server";

export class ClientEvent extends TreeDiagram.ServerEvent {
    public static eventTreeListUpdate = Symbol();
    public static eventTreeUpdate = Symbol();
    public static eventNodeUpdate = Symbol();

    public event: EventEmitter = new EventEmitter();

    public TreeListUpdate() {
        this.event.emit(ClientEvent.eventTreeListUpdate);
    }

    public TreeUpdate(uuid: string) {
        this.event.emit(ClientEvent.eventTreeUpdate, uuid);
    }

    public NodeUpdate(uuid: string, data: string) {
        this.event.emit(ClientEvent.eventNodeUpdate, uuid, data);
    }
}
