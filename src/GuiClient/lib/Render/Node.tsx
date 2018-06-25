import * as until from "util";

import * as React from "react";

import { getLogger } from "log4js";

import { TreeDiagram as TreeDiagram_Node } from "../../Ice/Node";
import { TreeDiagram as TreeDiagram_TreeView } from "../../Ice/TreeView";

import { ClientEvent } from "../IceProxy/ClientEvent";
import { Proxy } from "../IceProxy/IceProxy";

const logger = getLogger("Node");

export interface ITreeEditProps {
    rootUUID: string;
    parentUUID: string;
    isRoot: boolean;
    uuid: string;
    data: string;
    view: TreeDiagram_TreeView.TreeView | null;
}

export interface ITreeListState {
    data: string;
    childNodeData: string;
}

export class Node extends React.Component<ITreeEditProps, ITreeListState> {
    private proxy: Proxy | null = null;

    public constructor(props: ITreeEditProps) {
        super(props);
        this.state = {
            data: this.props.data,
            childNodeData: "",
        };
        this.init();
    }

    public renderChildren(nodes: TreeDiagram_Node.Node[]) {
        const rows: JSX.Element[] = [];
        let i = 0;
        for (const node of nodes) {
            rows.push(<li key={i++} className="list-group-item">
                <Node rootUUID={this.props.rootUUID} parentUUID={this.props.uuid} isRoot={false} uuid={node.uuid} data={node.data} view={this.props.view} />
            </li>);
        }
        return rows;
    }
    public render() {
        const nodes = this.findChildren();
        return <>
            {
                this.props.isRoot ? <></> :
                    <div>
                        節點資料： <input type="text" value={this.props.data} onChange={(e) => this.dataChange(e)} />
                    </div>
            }

            <div>
                新增子節點： <input type="text" value={this.state.childNodeData} onChange={(e) => this.childNodeDataChange(e)} />
                <button type="button" className="btn btn-defaults" onClick={() => { this.childNodeDataCrwate(); }}>新增</button>
            </div>
            {
                this.props.isRoot ? <></> :
                    <div className="btn-group btn-group-sm" role="group">
                        <button type="button" className="btn btn-secondary">刪除節點</button>
                        <button type="button" className="btn btn-secondary">刪除節點與子樹</button>
                    </div>
            }
            {
                nodes.length > 0 ? <>
                    <hr />
                    <ul className="list-group">{this.renderChildren(nodes)}</ul>
                </> : <></>
            }
        </>;
    }

    public componentWillUnmount() {
        if (this.proxy !== null) {
            this.proxy.event.event.removeListener(ClientEvent.eventNodeUpdate, this.eventNodeUpdateListener);
        }
    }

    private findChildren() {
        const nodes: TreeDiagram_Node.Node[] = [];
        const view = this.props.view;
        if (view !== null) {
            for (const rel of view.rels) {
                if (rel.parentUUID === this.props.uuid) {
                    const childIndex = view.nodes.findIndex((n) => {
                        return n.uuid === rel.childUUID;
                    });
                    if (childIndex >= 0) {
                        nodes.push(view.nodes[childIndex]);
                    }
                }
            }
        }
        // logger.info(until.inspect(view));
        // logger.info(until.inspect(nodes));
        return nodes;
    }

    private async dataChange(event: React.ChangeEvent<HTMLInputElement>) {
        // if (this.proxy !== null) {
        //     await this.proxy.server_updateNodeData(this.props.uuid, event.target.value);
        // }
        return;
    }

    private async childNodeDataChange(event: React.ChangeEvent<HTMLInputElement>) {
        this.setState({ childNodeData: event.target.value });
    }

    private async childNodeDataCrwate() {
        if (this.proxy !== null) {
            logger.info(`${this.props.rootUUID} ${this.props.uuid} ${this.state.childNodeData}`);
            await this.proxy.server_createNode(this.props.rootUUID, this.props.uuid, this.state.childNodeData);
        }
    }

    private eventNodeUpdateListener = (uuid: string, data: string) => {
        if (uuid === this.props.uuid) {
            this.setState({ data });
        }
    }

    private async init() {
        this.proxy = await Proxy.GetProxy();
        this.proxy.event.event.addListener(ClientEvent.eventNodeUpdate, this.eventNodeUpdateListener);
    }
}
