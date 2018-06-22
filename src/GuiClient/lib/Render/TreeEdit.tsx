import * as until from "util";

import * as React from "react";

import { TreeDiagram as TreeDiagram_Tree } from "../../Ice/Tree";
import { TreeDiagram as TreeDiagram_TreeView } from "../../Ice/TreeView";

import { ClientEvent } from "../IceProxy/ClientEvent";
import { Proxy } from "../IceProxy/IceProxy";
import { Tree } from "../IceProxy/Tree";

import { getLogger } from "log4js";

const logger = getLogger();

export interface ITreeEditProps {
    uuid: string;
    type: TreeDiagram_Tree.TreeType;
    CallBackHandler: () => void;
}

export interface ITreeListState {
    view: TreeDiagram_TreeView.TreeView | null;
}

export class TreeEdit extends React.Component<ITreeEditProps, ITreeListState> {
    private proxy: Proxy | null = null;

    public constructor(props: ITreeEditProps) {
        super(props);
        this.init();
        this.state = {
            view: null,
        };
    }

    public render() {
        const view = this.state.view;
        return <>
            <h1 >UUID = {this.props.uuid} {this.props.type.toString()}</h1>
            <h1 >type = {this.props.type.toString()}</h1>
            <hr />
            <button type="button" className="btn btn-success" onClick={this.props.CallBackHandler} >回圖清單</button>
        </>;
    }
    public componentWillUnmount() {
        if (this.proxy !== null) {
            this.proxy.event.event.removeListener(ClientEvent.eventTreeUpdate, this.eventTreeUpdateListener);
            this.proxy.event.event.removeListener(ClientEvent.eventNodeUpdate, this.eventNodeUpdateListener);
        }
    }
    private async deleteTree(uuid: string) {
        if (this.proxy !== null) {
            await this.proxy.server_deleteTree(uuid);
            alert("刪除完成");
        }
    }

    private eventTreeUpdateListener = () => {
        return;
    }
    private eventNodeUpdateListener = () => {
        return;
    }

    private async init() {
        this.proxy = await Proxy.GetProxy();
        this.proxy.event.event.addListener(ClientEvent.eventTreeUpdate, this.eventTreeUpdateListener);
        this.proxy.event.event.addListener(ClientEvent.eventNodeUpdate, this.eventNodeUpdateListener);
    }
}
