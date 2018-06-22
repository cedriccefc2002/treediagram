import * as until from "util";

import * as React from "react";

import { TreeDiagram as TreeDiagram_Tree } from "../../Ice/Tree";

import { ClientEvent } from "../IceProxy/ClientEvent";
import { Proxy } from "../IceProxy/IceProxy";
import { Tree } from "../IceProxy/Tree";

import { getLogger } from "log4js";

const logger = getLogger();

export interface IRenderProps {
    compiler: string;
    framework: string;
}

export interface IRenderState {
    trees: Tree[];
    createTree: {
        type: string;
    };
}

export class Render extends React.Component<IRenderProps, IRenderState> {
    private proxy: Proxy | null = null;

    public constructor(props: IRenderProps) {
        super(props);
        this.init();
        this.state = {
            trees: [],
            createTree: {
                type: "0",
            },
        };
    }
    public renderTress() {
        const rows: JSX.Element[] = [];
        let i = 1;
        for (const tree of this.state.trees) {
            const deletefn = () => { this.deleteTree(tree.uuid); };
            rows.push(<tr key={i}>
                <th scope="row">{i}</th>
                <td>{tree.uuid}</td>
                <td>{tree.type.toString() === "Binary" ? "二元樹" : "普通樹"}</td>
                <td>
                    <button type="button" className="btn btn-success">編輯</button>
                </td>
                <td>
                    <button type="button" className="btn btn-danger" onClick={deletefn} >刪除</button>
                </td>
            </tr>);
            i++;
        }
        return rows;
    }
    public render() {
        return <>
            <div className="form-inline">
                <div className="form-group mb-2">
                    <label>新建圖：</label>
                </div>
                <div className="form-group input-group mb-3">
                    <div className="input-group-prepend">
                        <label className="input-group-text" htmlFor="inputGroupSelect01">圖類型：</label>
                    </div>
                    <select className="custom-select" id="inputGroupSelect01" onChange={(e) => this.createTreeTypeChange(e)} value={this.state.createTree.type}>
                        <option value="0">普通樹</option>
                        <option value="1">二元樹</option>
                    </select>
                </div>
                <button type="submit" className="btn btn-primary mb-2" onClick={async () => { this.createTree(); }}>新增樹</button>
            </div>
            <hr />
            <table className="table table-striped table-bordered table-hover">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">UUID</th>
                        <th scope="col">普通樹或二元樹</th>
                        <th scope="col">編輯</th>
                        <th scope="col">刪除</th>
                    </tr>
                </thead>
                <tbody>{this.renderTress()}</tbody>
            </table>
        </>;
    }
    public componentWillUnmount() {
        if (this.proxy !== null) {
            this.proxy.event.event.removeListener(ClientEvent.eventTreeListUpdate, this.eventTreeListUpdateListener);
        }
    }

    private createTreeTypeChange(event: React.ChangeEvent<HTMLSelectElement>) {
        this.setState({ createTree: { type: event.target.value } });
    }

    private async createTree() {
        const type = this.state.createTree.type;
        if (this.proxy !== null) {
            await this.proxy.server_createTree(type === "0" ? TreeDiagram_Tree.TreeType.Normal : TreeDiagram_Tree.TreeType.Binary);
            alert("新增完成");
        }
    }
    private async deleteTree(uuid: string) {
        if (this.proxy !== null) {
            await this.proxy.server_deleteTree(uuid);
            alert("刪除完成");
        }
    }

    private eventTreeListUpdateListener = () => {
        return this.treeListUpdate();
    }

    private async init() {
        this.proxy = await Proxy.GetProxy();
        this.proxy.event.event.addListener(ClientEvent.eventTreeListUpdate, this.eventTreeListUpdateListener);
        return this.treeListUpdate();
    }
    private async treeListUpdate() {
        if (this.proxy !== null) {
            const trees = await this.proxy.server_listAllTrees();
            logger.info(until.inspect(trees));
            this.setState({ trees });
        }
    }
}
